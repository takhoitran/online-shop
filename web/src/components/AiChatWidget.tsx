import { useEffect, useRef, useState, type FormEvent } from 'react';
import { askProductAdvisor } from '../api/ai';
import { extractErrorMessage } from '../api/client';
import { useAuth } from '../auth/AuthContext';

interface ChatMessage {
  role: 'user' | 'bot';
  text: string;
}

const GUEST_SUGGESTIONS = [
  'What products do you have?',
  'Do you have anything under $20?',
  'How do I create an account?',
];

const BUYER_SUGGESTIONS = [
  'What products do you have?',
  'Show me items under $20',
  'What payment methods do you accept?',
];

const STAFF_SUGGESTIONS = [
  'What products are low on stock?',
  'List your best-selling items',
  "What's the price range of your catalog?",
];

function suggestionsForRole(role: string | undefined): string[] {
  if (role === 'Buyer') return BUYER_SUGGESTIONS;
  if (role === 'Seller' || role === 'Admin') return STAFF_SUGGESTIONS;
  return GUEST_SUGGESTIONS;
}

export function AiChatWidget() {
  const { user } = useAuth();
  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState('');
  const [sending, setSending] = useState(false);
  const sendingRef = useRef(false);

  async function sendMessage(message: string) {
    if (!message || sendingRef.current) return;

    sendingRef.current = true;
    setMessages((prev) => [...prev, { role: 'user', text: message }]);
    setInput('');
    setSending(true);
    try {
      const reply = await askProductAdvisor(message);
      setMessages((prev) => [...prev, { role: 'bot', text: reply }]);
    } catch (err) {
      setMessages((prev) => [
        ...prev,
        { role: 'bot', text: extractErrorMessage(err, 'Sorry, the AI assistant is not responding right now.') },
      ]);
    } finally {
      sendingRef.current = false;
      setSending(false);
    }
  }

  const sendMessageRef = useRef(sendMessage);
  sendMessageRef.current = sendMessage;

  useEffect(() => {
    function onOpenAi(e: Event) {
      const detail = (e as CustomEvent<{ message?: string }>).detail;
      setOpen(true);
      if (detail?.message?.trim()) {
        void sendMessageRef.current(detail.message.trim());
      }
    }
    window.addEventListener('open-ai-chat', onOpenAi);
    return () => window.removeEventListener('open-ai-chat', onOpenAi);
  }, []);

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    sendMessage(input.trim());
  }

  if (!open) {
    return (
      <button className="ai-chat-fab" onClick={() => setOpen(true)} aria-label="Open AI shopping assistant">
        🤖
      </button>
    );
  }

  return (
    <div className="ai-chat-panel">
      <div className="ai-chat-header">
        <span>🤖 Shopping assistant</span>
        <button className="ai-chat-close" onClick={() => setOpen(false)} aria-label="Close">
          ✕
        </button>
      </div>
      <div className="ai-chat-messages">
        {messages.length === 0 && (
          <>
            <p className="product-stock">Ask me about products, prices, or stock!</p>
            <div className="ai-chat-suggestions">
              {suggestionsForRole(user?.role).map((s) => (
                <button key={s} type="button" className="ai-chat-suggestion" onClick={() => sendMessage(s)}>
                  {s}
                </button>
              ))}
            </div>
          </>
        )}
        {messages.map((m, i) => (
          <div key={i} className={`ai-chat-bubble ${m.role}`}>
            {m.text}
          </div>
        ))}
        {sending && <div className="ai-chat-bubble bot">Thinking…</div>}
      </div>
      <form className="ai-chat-input" onSubmit={handleSubmit}>
        <input
          value={input}
          onChange={(e) => setInput(e.target.value)}
          placeholder="Type a question…"
          disabled={sending}
        />
        <button className="btn small" type="submit" disabled={sending}>
          Send
        </button>
      </form>
    </div>
  );
}
