import { fireEvent } from '@testing-library/svelte';

/** Klappt den ersten Abschnitt (Accordion) auf; der Inhalt wird erst dann gerendert. */
export async function oeffneAbschnitt(container: HTMLElement) {
    const kopf = container.querySelector('.bx--accordion__heading');
    if (!kopf) throw new Error('Kein Abschnitt gefunden.');
    await fireEvent.click(kopf);
}

/** Die Zeilen der ersten Tabelle (ohne Aufklapp-Zeilen). */
export function tabellenZeilen(container: HTMLElement) {
    return Array.from(
        container.querySelectorAll('tbody tr[data-parent-row="true"]')
    );
}

/** Der Knopf mit genau diesem Text (nicht eine gleich lautende Spaltenüberschrift), innerhalb von `container`. */
export function knopf(container: HTMLElement, text: string) {
    return Array.from(container.querySelectorAll('button')).find(
        (b) => b.textContent?.trim() === text
    );
}
