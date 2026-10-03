import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  ArrowLeft, ArrowUpRight, Check, CheckCircle2, Circle, ClipboardList,
  Clock3, Code2, FileText, FolderKanban, LoaderCircle, RefreshCw,
  Server, UserRound, Wrench,
} from 'lucide-react'
import './project-board.css'

const STATUS = {
  todo: { label: 'В очереди', icon: Circle },
  in_progress: { label: 'В работе', icon: Clock3 },
  review_requested: { label: 'На ревью', icon: ArrowUpRight },
  changes_requested: { label: 'Есть замечания', icon: Wrench },
  done: { label: 'Готово', icon: CheckCircle2 },
}

const AREA = {
  backend: { label: 'Backend', icon: Server },
  frontend: { label: 'Frontend', icon: Code2 },
  devops: { label: 'DevOps', icon: Wrench },
  fullstack: { label: 'Full stack', icon: FolderKanban },
}

async function readJson(url, options) {
  const response = await fetch(url, options)
  const body = await response.json().catch(() => null)
  if (!response.ok) throw new Error(body?.message || `Ошибка локальной доски (${response.status})`)
  return body
}

function initialTaskState() {
  return { version: 1, tasks: {} }
}

function inlineText(text, prefix) {
  const parts = []
  const pattern = /`([^`]+)`|\*\*([^*]+)\*\*/g
  let previous = 0
  let match
  while ((match = pattern.exec(text)) !== null) {
    if (match.index > previous) parts.push(text.slice(previous, match.index))
    parts.push(match[1]
      ? <code key={`${prefix}-${match.index}`}>{match[1]}</code>
      : <strong key={`${prefix}-${match.index}`}>{match[2]}</strong>)
    previous = pattern.lastIndex
  }
  if (previous < text.length) parts.push(text.slice(previous))
  return parts.length ? parts : text
}

function TaskDescription({ value }) {
  const blocks = (value || '').split(/\n\s*\n/).map((block) => block.trim()).filter(Boolean)
  return <div className="board-markdown">{blocks.map((block, index) => {
    const lines = block.split('\n')
    if (lines.every((line) => /^\s*[-*]\s+/.test(line))) {
      return <ul key={index}>{lines.map((line, lineIndex) => <li key={lineIndex}>{inlineText(line.replace(/^\s*[-*]\s+/, ''), `${index}-${lineIndex}`)}</li>)}</ul>
    }
    if (/^#{1,3}\s/.test(lines[0])) {
      const title = lines[0].replace(/^#{1,3}\s+/, '')
      const remainder = lines.slice(1).join(' ').trim()
      return <div className="board-description-block" key={index}><h4>{inlineText(title, `${index}-title`)}</h4>{remainder && <p>{inlineText(remainder, `${index}-body`)}</p>}</div>
    }
    return <p key={index}>{inlineText(lines.join(' '), `${index}-paragraph`)}</p>
  })}</div>
}

export default function ProjectBoard() {
  const [sections, setSections] = useState([])
  const [warnings, setWarnings] = useState([])
  const [taskState, setTaskState] = useState(initialTaskState)
  const [selectedSectionId, setSelectedSectionId] = useState('')
  const [selectedTaskId, setSelectedTaskId] = useState('')
  const [draftStatus, setDraftStatus] = useState('todo')
  const [draftNote, setDraftNote] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [savedLabel, setSavedLabel] = useState('Состояние сохраняется в проекте')

  const refreshTaskDefinitions = useCallback(async () => {
    try {
      const result = await readJson('/__project-board/tasks')
      setSections(result.sections || [])
      setWarnings(result.warnings || [])
      setError('')
      setSelectedSectionId((current) => current || result.sections?.[0]?.id || '')
    } catch (loadError) {
      setError(loadError.message || 'Не удалось прочитать задачи из папки project-tasks.')
    } finally {
      setLoading(false)
    }
  }, [])

  const refreshAll = useCallback(async () => {
    setLoading(true)
    setError('')
    try {
      const [definition, state] = await Promise.all([
        readJson('/__project-board/tasks'),
        readJson('/__project-board/state'),
      ])
      setSections(definition.sections || [])
      setWarnings(definition.warnings || [])
      setTaskState(state?.version === 1 && state.tasks ? state : initialTaskState())
      setSelectedSectionId((current) => current || definition.sections?.[0]?.id || '')
      setSavedLabel('Состояние загружено из проекта')
      setError('')
    } catch (loadError) {
      setError(loadError.message || 'Не удалось загрузить локальную доску.')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { refreshAll() }, [refreshAll])
  useEffect(() => {
    if (!import.meta.hot) return undefined
    const handleTaskFilesChanged = () => { refreshTaskDefinitions() }
    import.meta.hot.on('project-board:tasks-changed', handleTaskFilesChanged)
    return () => import.meta.hot.off('project-board:tasks-changed', handleTaskFilesChanged)
  }, [refreshTaskDefinitions])

  const selectedSection = sections.find((section) => section.id === selectedSectionId) || sections[0]
  const allTasks = useMemo(() => sections.flatMap((section) => section.tasks || []), [sections])
  const selectedTask = allTasks.find((task) => task.id === selectedTaskId)
  const selectedTaskRecord = selectedTask ? (taskState.tasks[selectedTask.id] || {}) : {}
  const totalDone = allTasks.filter((task) => taskState.tasks[task.id]?.status === 'done').length
  const reviewCount = allTasks.filter((task) => taskState.tasks[task.id]?.status === 'review_requested').length
  const activeCount = allTasks.filter((task) => taskState.tasks[task.id]?.status === 'in_progress').length
  const sectionDone = (section) => (section.tasks || []).filter((task) => taskState.tasks[task.id]?.status === 'done').length

  useEffect(() => {
    if (!selectedTask) return
    setDraftStatus(selectedTaskRecord.status || 'todo')
    setDraftNote(selectedTaskRecord.note || '')
  }, [selectedTask?.id, selectedTaskRecord.status, selectedTaskRecord.note])

  function openTask(task) {
    setSelectedTaskId(task.id)
    setDraftStatus(taskState.tasks[task.id]?.status || 'todo')
    setDraftNote(taskState.tasks[task.id]?.note || '')
  }

  async function saveTask(status = draftStatus) {
    if (!selectedTask) return
    setSaving(true)
    setError('')
    const nextState = {
      ...taskState,
      version: 1,
      tasks: {
        ...taskState.tasks,
        [selectedTask.id]: {
          ...selectedTaskRecord,
          status,
          note: draftNote.trim(),
          updatedAt: new Date().toISOString(),
        },
      },
    }
    try {
      await readJson('/__project-board/state', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(nextState),
      })
      setTaskState(nextState)
      setDraftStatus(status)
      setSavedLabel(`Сохранено ${new Intl.DateTimeFormat('ru-RU', { hour: '2-digit', minute: '2-digit' }).format(new Date())}`)
    } catch (saveError) {
      setError(saveError.message || 'Не удалось сохранить статус. Попробуйте ещё раз.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="project-board">
      <aside className="board-sidebar">
        <a className="board-brand" href="/" aria-label="SelfBudget — на главную"><span><FolderKanban size={19} /></span><b>Self<span>Budget</span></b></a>
        <div className="board-workspace-label">КОМАНДНАЯ ДОСКА</div>
        <div className="board-nav-current"><ClipboardList size={17} /><span>План проекта</span></div>
        <div className="board-sidebar-heading">РАЗДЕЛЫ</div>
        <nav className="board-section-nav" aria-label="Разделы проекта">
          {sections.map((section) => {
            const done = sectionDone(section)
            const total = section.tasks?.length || 0
            return <button type="button" key={section.id} className={selectedSection?.id === section.id ? 'active' : ''} onClick={() => { setSelectedSectionId(section.id); setSelectedTaskId('') }}>
              <span className="board-section-marker">{String(section.order || '').padStart(2, '0')}</span>
              <span className="board-section-link-copy"><b>{section.title}</b><small>{total ? `${done}/${total} задач` : 'Запланировано'}</small></span>
              {total > 0 && done === total ? <CheckCircle2 size={15} className="board-section-complete" /> : null}
            </button>
          })}
        </nav>
        <div className="board-sidebar-bottom">
          <div className="board-save-state"><span className="board-save-dot" /><span>{savedLabel}</span></div>
          <a className="board-back-link" href="/"><ArrowLeft size={15} /> Вернуться в SelfBudget</a>
        </div>
      </aside>

      <main className="board-main">
        <header className="board-topbar">
          <div className="board-breadcrumb"><span>SelfBudget</span><span>/</span><b>План проекта</b></div>
          <div className="board-top-actions"><span className="board-local-badge"><span /> Локальный проект</span><button className="board-refresh" type="button" onClick={refreshAll} disabled={loading}><RefreshCw size={15} className={loading ? 'board-spinning' : ''} /> Обновить</button></div>
        </header>

        <div className="board-content">
          <div className="board-intro-row">
            <div><div className="board-eyebrow"><span /> РОАДМАП И ТЕКУЩАЯ РАБОТА</div><h1>Строим банк <em>по шагам.</em></h1><p>Задачи загружаются из файлов проекта. Отметь готовность и оставь заметку — я сверю её с кодом.</p></div>
            <div className="board-map-icon"><FolderKanban size={22} /></div>
          </div>

          <section className="board-summary" aria-label="Сводка задач">
            <article><span className="board-summary-icon violet"><ClipboardList size={17} /></span><div><small>Всего задач</small><b>{allTasks.length}</b></div></article>
            <article><span className="board-summary-icon green"><CheckCircle2 size={17} /></span><div><small>Завершено</small><b>{totalDone}<small className="board-summary-total"> / {allTasks.length}</small></b></div></article>
            <article><span className="board-summary-icon amber"><Clock3 size={17} /></span><div><small>В работе</small><b>{activeCount}</b></div></article>
            <article><span className="board-summary-icon blue"><ArrowUpRight size={17} /></span><div><small>Ждут ревью</small><b>{reviewCount}</b></div></article>
          </section>

          {error && <div className="board-error" role="alert"><b>Доска недоступна</b><span>{error}. Открой страницу через локальный Vite-сервер `npm run dev`.</span></div>}
          {warnings.length > 0 && <details className="board-warnings"><summary>Не удалось прочитать {warnings.length} файлов</summary>{warnings.map((warning) => <p key={warning}>{warning}</p>)}</details>}

          {selectedSection ? <>
            <section className="board-feature-heading">
              <div><div className="board-section-kicker">РАЗДЕЛ {String(selectedSection.order || '').padStart(2, '0')} <span>·</span> {selectedSection.priority === 'mvp' ? 'MVP' : 'СЛЕДУЮЩИЙ ЭТАП'}</div><h2>{selectedSection.title}</h2><p>{selectedSection.summary}</p></div>
              <div className="board-feature-progress"><span>{sectionDone(selectedSection)} из {selectedSection.tasks?.length || 0} готово</span><div><i style={{ width: `${selectedSection.tasks?.length ? (sectionDone(selectedSection) / selectedSection.tasks.length) * 100 : 0}%` }} /></div></div>
            </section>

            <div className={`board-work-area ${selectedTask ? 'has-detail' : ''}`}>
              <section className="board-task-list" aria-label={`Задачи раздела ${selectedSection.title}`}>
                {loading ? <div className="board-loading"><LoaderCircle size={19} className="board-spinning" /> Загружаем файлы задач…</div> : selectedSection.tasks?.length ? selectedSection.tasks.map((task, index) => {
                  const taskStatus = taskState.tasks[task.id]?.status || 'todo'
                  const state = STATUS[taskStatus] || STATUS.todo
                  const StatusIcon = state.icon
                  const AreaIcon = AREA[task.area]?.icon || FileText
                  return <button type="button" className={`board-task-card ${selectedTaskId === task.id ? 'selected' : ''}`} key={task.id} onClick={() => openTask(task)}>
                    <span className="board-task-index">{String(index + 1).padStart(2, '0')}</span>
                    <span className="board-task-card-body"><span className="board-task-title-row"><b>{task.title}</b><span className={`board-status-pill ${taskStatus}`}><StatusIcon size={12} />{state.label}</span></span><span className="board-task-meta"><span><AreaIcon size={13} />{AREA[task.area]?.label || task.area || 'Задача'}</span><span><UserRound size={13} />{task.owner === 'assistant' ? 'Ассистент' : 'Ты'}</span><span className="board-size">{task.size || 'M'}</span></span></span>
                    <ArrowUpRight size={16} className="board-task-open" />
                  </button>
                }) : <div className="board-empty-tasks"><div className="board-empty-icon"><FileText size={19} /></div><b>Подробные задачи пока не добавлены</b><p>Добавь файл `*.task.md` в папку этого раздела — доска подхватит его автоматически.</p><code>project-tasks/{selectedSection.id}/</code></div>}
              </section>

              {selectedTask && <aside className="board-task-detail" aria-label={`Детали задачи ${selectedTask.title}`}>
                <div className="board-detail-top"><div className="board-detail-label">ТЕХНИЧЕСКАЯ ЗАДАЧА</div><button type="button" className="board-close-detail" onClick={() => setSelectedTaskId('')} aria-label="Закрыть задачу">×</button></div>
                <h3>{selectedTask.title}</h3>
                <div className="board-detail-meta"><span>{AREA[selectedTask.area]?.label || selectedTask.area}</span><span>{selectedTask.owner === 'assistant' ? 'Ассистент' : 'Твоя задача'}</span><span>Размер {selectedTask.size || 'M'}</span></div>
                <div className="board-description"><TaskDescription value={selectedTask.description} /></div>
                <div className="board-task-source"><FileText size={14} /> {selectedTask.file}</div>
                <label className="board-status-field">Статус
                  <select value={draftStatus} onChange={(event) => setDraftStatus(event.target.value)}>
                    {Object.entries(STATUS).map(([value, item]) => <option value={value} key={value}>{item.label}</option>)}
                  </select>
                </label>
                <label className="board-note-field">Что сделано / что проверять
                  <textarea value={draftNote} onChange={(event) => setDraftNote(event.target.value)} maxLength={5000} placeholder="Опиши результат, ограничения или файлы, на которые обратить внимание…" rows={5} />
                </label>
                <div className="board-note-hint">Заметка и статус сохраняются в `project-board-state.json`.</div>
                <div className="board-detail-actions">
                  <button type="button" className="board-save-button" onClick={() => saveTask()} disabled={saving}><Check size={15} />{saving ? 'Сохраняем…' : 'Сохранить'}</button>
                  <button type="button" className="board-review-button" onClick={() => saveTask('review_requested')} disabled={saving}><ArrowUpRight size={15} /> Готово к ревью</button>
                </div>
                {selectedTaskRecord.updatedAt && <small className="board-updated-at">Обновлено {new Intl.DateTimeFormat('ru-RU', { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(selectedTaskRecord.updatedAt))}</small>}
              </aside>}
            </div>
          </> : !loading && <div className="board-error"><b>Не найдены разделы</b><span>Создай папку <code>project-tasks/&lt;название-раздела&gt;/</code> и файл <code>_section.md</code>.</span></div>}

          <footer className="board-footer"><span>Файлы задач: <code>project-tasks/</code></span><span>Состояние: <code>project-board-state.json</code></span><span>После статуса «Готово к ревью» напиши в чате: <b>проверь доску</b>.</span></footer>
        </div>
      </main>
    </div>
  )
}
