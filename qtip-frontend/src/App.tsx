import { useEffect, useLayoutEffect, useMemo, useRef, useState } from 'react';
import './App.css';
import { getPiiEmailStats, submitText } from './api/qtipApi';

const EMAIL_REGEX = /[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}/gi;

function escapeHtml(input: string) {
    return input
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

function highlightEmailsAsHtml(text: string) {
    const normalised = text.replace(/\r\n/g, '\n');

    if (!normalised) return '';

    let out = '';
    let last = 0;

    for (const match of normalised.matchAll(EMAIL_REGEX)) {
        const email = match[0];
        const idx = match.index ?? 0;

        out += escapeHtml(normalised.slice(last, idx));
        out += `<span class="pii" title="PII – Email Address">${escapeHtml(email)}</span>`;

        last = idx + email.length;
    }

    out += escapeHtml(normalised.slice(last));

    return out;
}

function getCaretOffset(el: HTMLElement) {
    const sel = window.getSelection();
    if (!sel || sel.rangeCount === 0) return 0;

    const range = sel.getRangeAt(0);
    const pre = range.cloneRange();
    pre.selectNodeContents(el);
    pre.setEnd(range.endContainer, range.endOffset);
    return pre.toString().length;
}

function setCaretOffset(el: HTMLElement, offset: number) {
    const sel = window.getSelection();
    if (!sel) return;

    let current = 0;
    const walker = document.createTreeWalker(el, NodeFilter.SHOW_TEXT);

    while (walker.nextNode()) {
        const node = walker.currentNode as Text;
        const next = current + node.data.length;

        if (offset <= next) {
            const range = document.createRange();
            range.setStart(node, Math.max(0, offset - current));
            range.collapse(true);
            sel.removeAllRanges();
            sel.addRange(range);
            return;
        }

        current = next;
    }

    const range = document.createRange();
    range.selectNodeContents(el);
    range.collapse(false);
    sel.removeAllRanges();
    sel.addRange(range);
}

export default function App() {
    const editorRef = useRef<HTMLDivElement | null>(null);
    const caretRef = useRef<number>(0);

    const [text, setText] = useState('');
    const [total, setTotal] = useState<number>(0);
    const [lastDetected, setLastDetected] = useState<number>(0);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [isEditorFocused, setIsEditorFocused] = useState(false);
    const [submitMessage, setSubmitMessage] = useState<string | null>(null);

    const highlightedHtml = useMemo(() => highlightEmailsAsHtml(text), [text]);

    async function refreshStats() {
        const stats = await getPiiEmailStats(); // returns a number
        console.log(stats);
        setTotal(stats);
    }

    useEffect(() => {
        refreshStats().catch((e) => setError(e instanceof Error ? e.message : String(e)));
    }, []);

    useLayoutEffect(() => {
        const el = editorRef.current;
        if (!el) return;
        setCaretOffset(el, caretRef.current);
    }, [highlightedHtml]);

    useEffect(() => {
        if (!submitMessage) return;

        const timeoutId = window.setTimeout(() => {
            setSubmitMessage(null);
        }, 4000);

        return () => {
            window.clearTimeout(timeoutId);
        };
    }, [submitMessage]);

    function handleInput() {
        const el = editorRef.current;
        if (!el) return;

        caretRef.current = getCaretOffset(el);

        const next = el.innerText.replace(/\r\n/g, '\n');
        setText(next);
        setSubmitMessage(null);
        setError(null);
    }

    function handlePaste(e: React.ClipboardEvent<HTMLDivElement>) {
        e.preventDefault();
        const paste = e.clipboardData.getData('text/plain');
        document.execCommand('insertText', false, paste);
    }

    async function handleSubmit() {
        setError(null);
        setIsSubmitting(true);
        setSubmitMessage(null);

        try {
            const res = await submitText(text);

            const detected =
                typeof res.detectedCount === 'number'
                    ? res.detectedCount
                    : (text.match(EMAIL_REGEX)?.length ?? 0);

            setLastDetected(detected);

            if (typeof res.totalPiiEmailCount === 'number') {
                setTotal(res.totalPiiEmailCount);
            } else {
                await refreshStats();
            }

            setSubmitMessage('Submitted successfully!');

            setText('');
            caretRef.current = 0;
        } catch (e) {
            console.error(e);
            setError('Submission failed. Please try again.');
        } finally {
            setIsSubmitting(false);
        }
    }

    return (
        <div className="page">
            <h1>QTip</h1>

            <div className="panel">
                <div className="stat">
                    <div className="statLabel">Total PII emails submitted</div>
                    <div className="statValue">{total}</div>
                </div>

                <div className="stat">
                    <div className="statLabel">Detected in last submission</div>
                    <div className="statValue">{lastDetected}</div>
                </div>
            </div>

            <div className="editorWrap">
                <div
                    ref={editorRef}
                    className="editor"
                    contentEditable
                    suppressContentEditableWarning
                    spellCheck={false}
                    onInput={handleInput}
                    onPaste={handlePaste}
                    onFocus={() => setIsEditorFocused(true)}
                    onBlur={() => setIsEditorFocused(false)}
                    dangerouslySetInnerHTML={{ __html: text ? highlightedHtml : '' }}
                />
                {!text && !isEditorFocused && (
                    <div className="editorPlaceholder">Paste or type text here…</div>
                )}
            </div>

            <div className="actions">
                <button onClick={handleSubmit} disabled={isSubmitting || text.trim().length === 0}>
                    {isSubmitting ? 'Submitting…' : 'Submit'}
                </button>
            </div>

            {submitMessage && !error && (
                <div className="success">{submitMessage}</div>
            )}

            {error && <div className="error">{error}</div>}
        </div>
    );
}
