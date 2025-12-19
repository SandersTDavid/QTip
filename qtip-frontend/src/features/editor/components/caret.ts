export function getCaretOffset(el: HTMLElement) {
    const sel = window.getSelection();
    if (!sel || sel.rangeCount === 0) return 0;

    const range = sel.getRangeAt(0);
    const pre = range.cloneRange();
    pre.selectNodeContents(el);
    pre.setEnd(range.endContainer, range.endOffset);
    return pre.toString().length;
}

export function setCaretOffset(el: HTMLElement, offset: number) {
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
