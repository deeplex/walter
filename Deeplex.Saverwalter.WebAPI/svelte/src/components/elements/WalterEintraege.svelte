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
    Die eine Form für Einträge unter einer Tabellenzeile oder unter einer Überschrift:
    Datum, Betrag bzw. Wert, Text (verlinkt), Tag und Aktion in festen Spalten, darunter eingerückt
    die Teile. Datum und Betrag dürfen fehlen; die Spalten bleiben trotzdem bündig.
    Aktionen je Eintrag über den Slot `aktion` (let:eintrag). Optional darüber eine Notiz
    und darunter der Link zur eigenen Seite mit der Zahl der Anhänge, die beim Anzeigen
    (also beim Aufklappen) geladen wird.
-->
<script lang="ts">
    import { formatEuro, formatDatum } from '$walter/services/format';
    import { Tag } from 'carbon-components-svelte';
    import { Attachment } from 'carbon-icons-svelte';
    import { onMount } from 'svelte';
    import { walter_fetch } from '$walter/services/requests';
    import type { WalterEintrag } from '$walter/lib';

    type E = $$Generic<WalterEintrag>;

    export let eintraege: E[];
    /** Text, wenn es keine Einträge gibt; ohne Text erscheint dann nichts. */
    export let leerText: string | undefined = undefined;
    /** Link unter der Liste, z. B. „Zähler öffnen →“ zur eigenen Seite. */
    export let mehrHref: string | undefined = undefined;
    export let mehrText = 'Öffnen →';
    /** Notiz des Objekts (z. B. der Transaktion), sonst nur auf der eigenen Seite sichtbar. */
    export let notiz: string | null | undefined = undefined;
    /** Datei-Endpunkt des Objekts (`…/{id}/files`): zeigt die Zahl der Anhänge am Link. */
    export let anhaengeUrl: string | undefined = undefined;
    export let fetchImpl: typeof fetch = fetch;

    // Ohne Antwort (z. B. Dateiablage nicht erreichbar) erscheint einfach keine Zahl.
    let anhaenge = 0;
    onMount(async () => {
        if (!anhaengeUrl) return;
        try {
            const response = await walter_fetch(fetchImpl, anhaengeUrl);
            if (response.ok) {
                const dateien = (await response.json()) as unknown[];
                anhaenge = Array.isArray(dateien) ? dateien.length : 0;
            }
        } catch {
            anhaenge = 0;
        }
    });
</script>

{#if notiz?.trim()}
    <p class="notiz"><span class="notiz-titel">Notiz:</span> {notiz}</p>
{/if}

{#if eintraege.length === 0}
    {#if leerText}
        <p class="leer">{leerText}</p>
    {/if}
{:else}
    <ul class="eintraege">
        {#each eintraege as eintrag (eintrag.id)}
            <li class="eintrag">
                <div class="zeile">
                    <span class="datum">
                        {eintrag.datum ? formatDatum(eintrag.datum) : ''}
                    </span>
                    <span class="betrag">
                        {eintrag.betrag !== undefined
                            ? formatEuro(eintrag.betrag)
                            : (eintrag.wert ?? '')}
                    </span>
                    <span class="text">
                        {#if eintrag.href}
                            <a href={eintrag.href}>{eintrag.text}</a>
                        {:else}
                            {eintrag.text}
                        {/if}
                    </span>
                    <span class="tag">
                        {#if eintrag.tag}
                            <Tag size="sm" type={eintrag.tag.tag}
                                >{eintrag.tag.text}</Tag
                            >
                        {/if}
                    </span>
                    <span class="aktion"><slot name="aktion" {eintrag} /></span>
                </div>
                {#if eintrag.teile && eintrag.teile.length > 0}
                    <ul class="teile">
                        {#each eintrag.teile as teil}
                            <li class="zeile teil" class:gedimmt={teil.gedimmt}>
                                <span class="datum" />
                                <span class="betrag">
                                    {teil.betrag !== undefined
                                        ? formatEuro(teil.betrag)
                                        : ''}
                                </span>
                                <span class="text">
                                    {#if teil.href}
                                        <a href={teil.href}>{teil.text}</a>
                                    {:else}
                                        {teil.text}
                                    {/if}
                                </span>
                            </li>
                        {/each}
                    </ul>
                {/if}
            </li>
        {/each}
    </ul>
{/if}
{#if mehrHref}
    <div class="mehr">
        <a href={mehrHref}>{mehrText}</a>
        {#if anhaenge > 0}
            <a class="anhaenge" href={mehrHref}>
                <Attachment size={16} />
                {anhaenge === 1 ? '1 Anhang' : `${anhaenge} Anhänge`}
            </a>
        {/if}
    </div>
{/if}

<style>
    ul {
        list-style: none;
        margin: 0;
        padding: 0;
    }
    .eintraege {
        width: 100%;
    }
    .eintrag {
        padding: 0.375rem 0;
        border-bottom: 1px solid var(--cds-ui-03);
    }
    .eintrag:last-child {
        border-bottom: none;
    }
    .zeile {
        display: grid;
        grid-template-columns: 6.5rem 7.5rem minmax(0, 1fr) auto auto;
        grid-template-areas: 'datum betrag text tag aktion';
        column-gap: 1rem;
        align-items: center;
        min-height: 2rem;
    }
    .datum {
        grid-area: datum;
    }
    .text {
        grid-area: text;
    }
    .tag {
        grid-area: tag;
    }
    .aktion {
        grid-area: aktion;
    }
    .betrag {
        grid-area: betrag;
        text-align: right;
        font-variant-numeric: tabular-nums;
    }
    .text {
        overflow-wrap: anywhere;
    }
    .teil {
        min-height: 1.5rem;
        font-size: 0.875rem;
        color: var(--cds-text-02);
    }
    .teil .text {
        padding-left: 1rem;
        border-left: 2px solid var(--cds-ui-03);
    }
    .gedimmt {
        opacity: 0.6;
    }
    a {
        color: var(--cds-link-01);
        text-decoration: none;
    }
    a:hover {
        text-decoration: underline;
    }
    .mehr {
        display: flex;
        gap: 1.5rem;
        align-items: center;
        margin-top: 0.5rem;
    }
    .anhaenge {
        display: inline-flex;
        gap: 0.25rem;
        align-items: center;
    }
    .notiz {
        margin: 0.25rem 0 0.5rem 0;
        padding-left: 0.75rem;
        border-left: 2px solid var(--cds-ui-03);
        font-size: 0.875rem;
        color: var(--cds-text-02);
        white-space: pre-line;
    }
    .notiz-titel {
        font-weight: 600;
    }
    .leer {
        margin: 0;
        padding: 0.375rem 0;
        font-size: 0.875rem;
        color: var(--cds-text-02);
    }
    /* Schmal: Datum, Betrag und Aktion oben, der Text darunter über die ganze Breite. */
    @media (max-width: 42rem) {
        .zeile {
            grid-template-columns: auto auto minmax(0, 1fr) auto;
            grid-template-areas:
                'datum betrag tag aktion'
                'text text text text';
            row-gap: 0.125rem;
        }
        .teil {
            grid-template-columns: 5.5rem minmax(0, 1fr);
            grid-template-areas: 'betrag text';
        }
        .teil .datum {
            display: none;
        }
    }
</style>
