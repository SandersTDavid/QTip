import { useEffect, useMemo, useRef, useState } from 'react';
import { getPiiEmailStats, submitText } from '../api/qtipApi';
import { highlightEmailsAsHtml } from '../features/editor/components/emailHighlighter';
import { getCaretOffset, setCaretOffset } from '../features/editor/components/caret';

export default function Dashboard() {
    const editorRef = useRef<HTMLDivElement | null>(null);
    const caretRef = useRef<number>(0);

    const [text, setText] = useState('');
    const [total, setTotal] = useState<number>(0);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [isEditorFocused, setIsEditorFocused] = useState(false);
    const [submitMessage, setSubmitMessage] = useState<string | null>(null);

    const highlightedHtml = useMemo(() => highlightEmailsAsHtml(text), [text]);

    async function refreshStats() {
        const stats = await getPiiEmailStats();
        setTotal(stats.totalPiiEmailCount);
    }

    useEffect(() => {
        refreshStats().catch((e) =>
            setError(e instanceof Error ? e.message : String(e)),
        );
    }, []);

    useEffect(() => {
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
                <button
                    onClick={handleSubmit}
                    disabled={isSubmitting || text.trim().length === 0}
                >
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
