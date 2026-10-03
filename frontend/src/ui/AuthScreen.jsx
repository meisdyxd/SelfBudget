import { useState } from 'react'
import {
  ArrowRight, CheckCircle2, Eye, EyeOff, LockKeyhole,
  LoaderCircle, ShieldCheck, Sparkles, Wallet,
} from 'lucide-react'
import { loginUser, registerUser } from '../api/auth.js'
import { saveSession } from './session.js'
import './auth.css'

function localDateToday() {
  const now = new Date()
  return new Date(now.getTime() - now.getTimezoneOffset() * 60_000).toISOString().slice(0, 10)
}

function getInitialMode() {
  return new URLSearchParams(window.location.search).get('mode') === 'register' ? 'register' : 'login'
}

export default function AuthScreen() {
  const [mode, setMode] = useState(getInitialMode)
  const [form, setForm] = useState({ name: '', email: '', birthdate: '', password: '', passwordConfirmation: '' })
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirmation, setShowConfirmation] = useState(false)
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const [notice, setNotice] = useState(() => new URLSearchParams(window.location.search).get('reason') === 'expired'
    ? 'Сеанс завершён. Войдите снова, чтобы открыть своё пространство.'
    : '')

  function changeMode(nextMode) {
    setMode(nextMode)
    setError('')
    setNotice('')
    window.history.replaceState(null, '', `/auth?mode=${nextMode}`)
  }

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setNotice('')

    if (mode === 'register' && form.password !== form.passwordConfirmation) {
      setError('Пароли не совпадают.')
      return
    }
    if (mode === 'register' && form.password.length < 12) {
      setError('Используйте пароль длиной не менее 12 символов.')
      return
    }

    setPending(true)
    try {
      if (mode === 'register') {
        await registerUser({
          name: form.name.trim(),
          email: form.email.trim(),
          birthdate: form.birthdate,
          password: form.password,
        })
        setForm((current) => ({ ...current, password: '', passwordConfirmation: '' }))
        changeMode('login')
        setNotice('Аккаунт создан. Теперь войдите в SelfBudget.')
        return
      }

      const session = await loginUser({
        email: form.email.trim(),
        password: form.password,
      })
      if (!session?.accessToken) throw new Error('Сервер не вернул токен доступа. Попробуйте позже.')
      saveSession(session)
      window.location.assign('/')
    } catch (requestError) {
      setError(requestError.message || 'Не удалось выполнить запрос. Повторите попытку.')
    } finally {
      setPending(false)
    }
  }

  return (
    <main className="auth-page">
      <section className="auth-story" aria-label="О SelfBudget">
        <div className="auth-brand" aria-label="SelfBudget">
          <span className="auth-brand-mark"><Wallet size={20} strokeWidth={2.2} /></span>
          <span>self<span>budget</span></span>
        </div>

        <div className="auth-story-content">
          <span className="auth-eyebrow"><Sparkles size={14} /> ЛИЧНЫЕ ФИНАНСЫ В ЯСНОСТИ</span>
          <h1>Спокойствие начинается<br />с <em>понятных денег.</em></h1>
          <p>Счета, переводы и история операций — в одном пространстве, которое помогает держать финансовые дела под контролем.</p>
          <div className="auth-points">
            <div><span><ShieldCheck size={17} /></span><p><b>Ваше пространство</b><small>Личные данные доступны только вам.</small></p></div>
            <div><span><LockKeyhole size={17} /></span><p><b>Защищённый вход</b><small>Вход по почте и паролю.</small></p></div>
          </div>
        </div>

        <div className="auth-demo-note"><span className="auth-demo-dot" /> Учебный проект: здесь используются демонстрационные счета и операции.</div>
        <div className="auth-decoration auth-decoration-one" />
        <div className="auth-decoration auth-decoration-two" />
      </section>

      <section className="auth-panel" aria-labelledby="auth-title">
        <div className="auth-card">
          <div className="auth-card-icon"><LockKeyhole size={19} /></div>
          <div className="auth-kicker">ВАШЕ ФИНАНСОВОЕ ПРОСТРАНСТВО</div>
          <h2 id="auth-title">{mode === 'login' ? 'С возвращением' : 'Создайте аккаунт'}</h2>
          <p className="auth-subtitle">{mode === 'login' ? 'Войдите, чтобы увидеть свои счета.' : 'Заполните данные, чтобы начать.'}</p>

          <div className="auth-tabs" role="group" aria-label="Действие с аккаунтом">
            <button type="button" aria-pressed={mode === 'login'} className={mode === 'login' ? 'selected' : ''} onClick={() => changeMode('login')}>Войти</button>
            <button type="button" aria-pressed={mode === 'register'} className={mode === 'register' ? 'selected' : ''} onClick={() => changeMode('register')}>Регистрация</button>
          </div>

          {notice && <div className="auth-notice" role="status"><CheckCircle2 size={17} />{notice}</div>}
          {error && <div className="auth-error" role="alert">{error}</div>}

          <form className="auth-form" onSubmit={handleSubmit} aria-busy={pending}>
            {mode === 'register' && <>
              <label className="auth-field">Имя
                <input autoComplete="name" maxLength="100" value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} placeholder="Как к вам обращаться" required />
              </label>
              <label className="auth-field">Дата рождения
                <input type="date" autoComplete="bday" max={localDateToday()} value={form.birthdate} onChange={(event) => setForm({ ...form, birthdate: event.target.value })} required />
              </label>
            </>}

            <label className="auth-field">Электронная почта
              <input type="email" autoComplete="email" maxLength="254" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} placeholder="name@example.com" required />
            </label>

            <div className="auth-field">
              <label htmlFor="auth-password">Пароль</label>
              <span className="auth-password-wrap">
                <input id="auth-password" type={showPassword ? 'text' : 'password'} autoComplete={mode === 'login' ? 'current-password' : 'new-password'} minLength={mode === 'register' ? 12 : undefined} value={form.password} onChange={(event) => setForm({ ...form, password: event.target.value })} placeholder={mode === 'register' ? 'Не менее 12 символов' : 'Введите пароль'} aria-describedby={mode === 'register' ? 'auth-password-hint' : undefined} required />
                <button type="button" aria-label={showPassword ? 'Скрыть пароль' : 'Показать пароль'} aria-pressed={showPassword} aria-controls="auth-password" onClick={() => setShowPassword((visible) => !visible)}>{showPassword ? <EyeOff size={17} /> : <Eye size={17} />}</button>
              </span>
            </div>

            {mode === 'register' && <div className="auth-field">
              <label htmlFor="auth-password-confirmation">Повторите пароль</label>
              <span className="auth-password-wrap">
                <input id="auth-password-confirmation" type={showConfirmation ? 'text' : 'password'} autoComplete="new-password" minLength="12" value={form.passwordConfirmation} onChange={(event) => setForm({ ...form, passwordConfirmation: event.target.value })} placeholder="Введите пароль ещё раз" aria-describedby="auth-password-hint" required />
                <button type="button" aria-label={showConfirmation ? 'Скрыть пароль' : 'Показать пароль'} aria-pressed={showConfirmation} aria-controls="auth-password-confirmation" onClick={() => setShowConfirmation((visible) => !visible)}>{showConfirmation ? <EyeOff size={17} /> : <Eye size={17} />}</button>
              </span>
            </div>}

            {mode === 'register' && <p id="auth-password-hint" className="auth-password-hint"><LockKeyhole size={14} /> Для защиты аккаунта используйте не менее 12 символов.</p>}
            <button className="auth-submit" type="submit" disabled={pending}>
              {pending ? <><LoaderCircle size={17} className="auth-spinner" /><span>{mode === 'login' ? 'Входим…' : 'Создаём аккаунт…'}</span></> : <>{mode === 'login' ? 'Войти в аккаунт' : 'Создать аккаунт'} <ArrowRight size={16} /></>}
            </button>
          </form>

          <div className="auth-privacy"><ShieldCheck size={15} /><span>Пароль передаётся только API SelfBudget и не сохраняется в браузере.</span></div>
        </div>
        <footer className="auth-footer">Продолжая, вы используете демонстрационный финансовый сервис SelfBudget.</footer>
      </section>
    </main>
  )
}
