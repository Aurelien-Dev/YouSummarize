export function scrollIntoView(element) {
    if (!element) return;
    element.scrollIntoView({ behavior: 'smooth', block: 'end' });
}
