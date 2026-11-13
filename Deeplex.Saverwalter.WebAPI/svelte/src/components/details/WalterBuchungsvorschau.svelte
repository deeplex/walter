<!-- Copyright (C) 2023-2026  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<!--
    Zeigt vor dem Speichern, was eine Zahlung bewirkt — in Klartext vom Server
    (Trockenlauf, gleiche Prüfungen wie das Buchen). Die Buchungssätze lassen sich
    aufklappen. `gueltig` ist true, sobald der Server die Eingabe akzeptiert.
-->
<script lang="ts">
    import {
        Accordion,
        AccordionItem,
        InlineLoading,
        InlineNotification
    } from 'carbon-components-svelte';
    import type { TransaktionsInput, TransaktionsVorschau } from '$walter/lib';
    import { walter_post } from '$walter/services/requests';
    import { onDestroy } from 'svelte';

    /** Eingabe; `undefined`, solange sie unvollständig ist. */
    export let input: TransaktionsInput | undefined;
    export let gueltig = false;

    let vorschau: TransaktionsVorschau | undefined;
    let fehler: string | null = null;
    let laedt = false;
    let timer: ReturnType<typeof setTimeout> | undefined;
    let anfrage = 0;

    const eur = (v: number) =>
        v.toLocaleString('de-DE', { style: 'currency', currency: 'EUR' });
    const datumText = (d: string) => new Date(d).toLocaleDateString('de-DE');

    async function lade(eingabe: TransaktionsInput, nr: number) {
        laedt = true;
        try {
            const response = await walter_post(
                '/api/transaktionen/buchen/vorschau',
                eingabe
            );
            if (nr !== anfrage) return;
            if (response.ok) {
                vorschau = (await response.json()) as TransaktionsVorschau;
                fehler = null;
                gueltig = true;
            } else {
                vorschau = undefined;
                fehler = await response.text();
                gueltig = false;
            }
        } finally {
            if (nr === anfrage) laedt = false;
        }
    }

    // Kurz warten, damit nicht jeder Tastendruck eine Anfrage auslöst.
    $: {
        const eingabe = input ? JSON.parse(JSON.stringify(input)) : undefined;
        clearTimeout(timer);
        const nr = ++anfrage;
        gueltig = false;
        if (!eingabe) {
            vorschau = undefined;
            fehler = null;
            laedt = false;
        } else {
            timer = setTimeout(() => lade(eingabe, nr), 300);
        }
    }

    onDestroy(() => clearTimeout(timer));
</script>

<div class="vorschau">
    <h5>Vorschau</h5>
    {#if !input}
        <p class="hinweis">Datum und Betrag eingeben.</p>
    {:else if laedt && !vorschau && !fehler}
        <InlineLoading description="Prüfe…" />
    {:else if fehler}
        <InlineNotification
            kind="error"
            title="So nicht buchbar:"
            subtitle={fehler}
            hideCloseButton
            lowContrast
        />
    {:else if vorschau && input}
        <p>
            Zahlung <strong>{eur(vorschau.betrag)}</strong> am {datumText(
                input.zahlungsdatum
            )}
            {#if vorschau.zahler}von {vorschau.zahler}{/if}
        </p>
        <ul>
            {#each vorschau.positionen as p}
                <li>
                    → {eur(p.betrag)}
                    {p.text}
                    {#if p.danachOffen != null}
                        <span class="hinweis"
                            >(danach offen: {eur(p.danachOffen)})</span
                        >
                    {/if}
                </li>
            {/each}
        </ul>
        <Accordion size="sm">
            <AccordionItem title="Buchungssätze anzeigen">
                {#each vorschau.saetze as satz}
                    <p class="satz">
                        {datumText(satz.buchungsdatum)} · {satz.beschreibung}
                        {#if !satz.istZahlung}
                            <span class="hinweis">(entsteht mit)</span>
                        {/if}
                    </p>
                    <table>
                        {#each satz.zeilen as z}
                            <tr>
                                <td>{z.sollHaben}</td>
                                <td>{z.kontonummer} {z.kontobezeichnung}</td>
                                <td class="betrag">{eur(z.betrag)}</td>
                            </tr>
                        {/each}
                    </table>
                {/each}
                {#if vorschau.ausgleiche > 0}
                    <p class="hinweis">
                        {vorschau.ausgleiche} Ausgleich(e) mit offenen Posten.
                    </p>
                {/if}
            </AccordionItem>
        </Accordion>
    {/if}
</div>

<style>
    .vorschau {
        margin-top: 1rem;
        padding: 1rem;
        background: var(--cds-layer-accent, var(--cds-ui-02));
    }
    h5 {
        margin-bottom: 0.5rem;
    }
    ul {
        list-style: none;
        margin: 0.5rem 0;
        padding: 0;
    }
    .hinweis {
        color: var(--cds-text-secondary);
    }
    .satz {
        margin-top: 0.5rem;
    }
    table {
        width: 100%;
    }
    td {
        padding: 0.125rem 0.5rem 0.125rem 0;
    }
    .betrag {
        text-align: right;
    }
</style>
