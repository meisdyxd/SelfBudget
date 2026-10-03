import { readdir, readFile, rename, writeFile } from 'node:fs/promises'
import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

const projectRoot = path.dirname(fileURLToPath(import.meta.url))
const workspaceRoot = path.resolve(projectRoot, '..')
const tasksRoot = path.join(workspaceRoot, 'project-tasks')
const statePath = path.join(workspaceRoot, 'project-board-state.json')

function parseTaskFile(source) {
  const normalized = source.replace(/\r\n/g, '\n')
  const match = normalized.match(/^---\n([\s\S]*?)\n---\n?([\s\S]*)$/)
  if (!match) throw new Error('Не найден YAML-блок метаданных между строками ---')

  const metadata = {}
  for (const line of match[1].split('\n')) {
    const separator = line.indexOf(':')
    if (separator < 1) continue
    const key = line.slice(0, separator).trim()
    let value = line.slice(separator + 1).trim()
    if ((value.startsWith('"') && value.endsWith('"')) || (value.startsWith("'") && value.endsWith("'"))) {
      value = value.slice(1, -1)
    }
    if (value === 'true') metadata[key] = true
    else if (value === 'false') metadata[key] = false
    else if (/^-?\d+$/.test(value)) metadata[key] = Number(value)
    else metadata[key] = value
  }

  return { metadata, body: match[2].trim() }
}

async function discoverProjectTasks() {
  const entries = await readdir(tasksRoot, { withFileTypes: true })
  const sections = []
  const warnings = []
  const taskIds = new Set()

  for (const entry of entries.filter((item) => item.isDirectory())) {
    const sectionPath = path.join(tasksRoot, entry.name)
    const sectionFile = path.join(sectionPath, '_section.md')
    let section
    try {
      const parsed = parseTaskFile(await readFile(sectionFile, 'utf8'))
      section = parsed.metadata
      section.summary = parsed.body
    } catch (error) {
      warnings.push(`${entry.name}/_section.md: ${error.message}`)
      continue
    }

    const taskFiles = (await readdir(sectionPath, { withFileTypes: true }))
      .filter((file) => file.isFile() && file.name.endsWith('.task.md'))
      .sort((a, b) => a.name.localeCompare(b.name, 'ru'))
    const tasks = []

    for (const file of taskFiles) {
      const relativePath = path.posix.join('project-tasks', entry.name, file.name)
      try {
        const parsed = parseTaskFile(await readFile(path.join(sectionPath, file.name), 'utf8'))
        const task = { ...parsed.metadata, description: parsed.body, file: relativePath }
        if (!task.id || !task.title) throw new Error('У задачи обязательны поля id и title')
        if (taskIds.has(task.id)) throw new Error(`Идентификатор задачи ${task.id} уже используется`)
        taskIds.add(task.id)
        tasks.push(task)
      } catch (error) {
        warnings.push(`${relativePath}: ${error.message}`)
      }
    }

    sections.push({ ...section, tasks: tasks.sort((a, b) => (a.order || 0) - (b.order || 0)) })
  }

  sections.sort((a, b) => (a.order || 0) - (b.order || 0))
  return { sections, warnings }
}

function isLoopbackRequest(request) {
  const address = request.socket.remoteAddress
  return address === '127.0.0.1' || address === '::1' || address === '::ffff:127.0.0.1'
}

function sendJson(response, status, value) {
  response.statusCode = status
  response.setHeader('Content-Type', 'application/json; charset=utf-8')
  response.setHeader('Cache-Control', 'no-store')
  response.end(JSON.stringify(value))
}

async function readRequestBody(request, maxBytes = 512_000) {
  const chunks = []
  let length = 0
  for await (const chunk of request) {
    length += chunk.length
    if (length > maxBytes) throw new Error('Состояние доски слишком большое')
    chunks.push(chunk)
  }
  return JSON.parse(Buffer.concat(chunks).toString('utf8'))
}

async function readBoardState() {
  try {
    const state = JSON.parse(await readFile(statePath, 'utf8'))
    if (!state || state.version !== 1 || !state.tasks || typeof state.tasks !== 'object' || Array.isArray(state.tasks)) {
      throw new Error('Файл project-board-state.json имеет неверный формат')
    }
    return state
  } catch (error) {
    if (error.code === 'ENOENT') return { version: 1, tasks: {} }
    throw new Error(`Не удалось прочитать project-board-state.json: ${error.message}`)
  }
}

function projectBoardPlugin() {
  const attachApi = (server) => {
    server.middlewares.use(async (request, response, next) => {
      const url = new URL(request.url || '/', 'http://localhost')
      if (url.pathname !== '/__project-board/tasks' && url.pathname !== '/__project-board/state') return next()
      if (!isLoopbackRequest(request)) return sendJson(response, 403, { message: 'Локальный доступ разрешён только с этого компьютера.' })

      try {
        if (url.pathname === '/__project-board/tasks' && request.method === 'GET') {
          return sendJson(response, 200, await discoverProjectTasks())
        }
        if (url.pathname === '/__project-board/state' && request.method === 'GET') {
          return sendJson(response, 200, await readBoardState())
        }
        if (url.pathname === '/__project-board/state' && request.method === 'PUT') {
          const state = await readRequestBody(request)
          if (!state || state.version !== 1 || !state.tasks || typeof state.tasks !== 'object' || Array.isArray(state.tasks)) {
            return sendJson(response, 400, { message: 'Неверный формат состояния доски.' })
          }
          const temporaryPath = `${statePath}.tmp`
          await writeFile(temporaryPath, `${JSON.stringify(state, null, 2)}\n`, 'utf8')
          await rename(temporaryPath, statePath)
          return sendJson(response, 200, { saved: true })
        }
        return sendJson(response, 405, { message: 'Метод не поддерживается.' })
      } catch (error) {
        return sendJson(response, error instanceof SyntaxError ? 400 : 500, { message: error.message || 'Не удалось обработать запрос доски.' })
      }
    })
  }

  return {
    name: 'selfbudget-project-board',
    configureServer(server) {
      server.watcher.add(tasksRoot)

      const announceTaskChange = (changedPath) => {
        const normalizedTaskRoot = path.resolve(tasksRoot).toLowerCase()
        const normalizedChangedPath = path.resolve(changedPath).toLowerCase()
        if (normalizedChangedPath.startsWith(`${normalizedTaskRoot}${path.sep}`) && normalizedChangedPath.endsWith('.md')) {
          server.ws.send({ type: 'custom', event: 'project-board:tasks-changed' })
        }
      }
      server.watcher.on('add', announceTaskChange)
      server.watcher.on('change', announceTaskChange)
      server.watcher.on('unlink', announceTaskChange)
      attachApi(server)
    },
    configurePreviewServer: attachApi,
  }
}

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, projectRoot, '')
  const apiTarget = env.SELFBUDGET_API_PROXY_TARGET || 'https://localhost:7023'
  const isLocalHttps = /^https:\/\/(localhost|127(?:\.\d+){3}|\[::1\])(?::\d+)?$/i.test(apiTarget)
  const apiProxy = {
    target: apiTarget,
    changeOrigin: true,
    secure: !isLocalHttps,
  }

  return {
    plugins: [react(), projectBoardPlugin()],
    server: {
      host: '127.0.0.1',
      port: 5173,
      fs: { allow: [projectRoot] },
      proxy: { '/api': apiProxy },
    },
    preview: {
      host: '127.0.0.1',
      port: 4173,
      proxy: { '/api': apiProxy },
    },
  }
})
