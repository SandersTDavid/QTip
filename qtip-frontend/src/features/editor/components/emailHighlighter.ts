export const EMAIL_REGEX = /[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}/gi;

function escapeHtml(input: string) {
    return input
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

export function highlightEmailsAsHtml(text: string) {
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
