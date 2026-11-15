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
    Aktionen je Eintrag über den Slot `aktion` (let:item). Optional darüber eine Notiz
    und darunter der Link zur eigenen Seite mit der Zahl der Anhänge, die beim Anzeigen
    (also beim Aufklappen) geladen wird.
-->
<script lang="ts">
    import { formatEuro, formatDate } from '$walter/services/format';
    import { Tag } from 'carbon-components-svelte';
    import { Attachment } from 'carbon-icons-svelte';
    import { onMount } from 'svelte';
    import { walter_fetch } from '$walter/services/requests';
    import type { WalterListItem } from '$walter/lib';

    type E = $$Generic<WalterListItem>;

    export let items: E[];
    /** Text, wenn es keine Einträge gibt; ohne Text erscheint dann nichts. */
    export let leerText: string | undefined = undefined;
    /** Link unter der Liste, z. B. „Zähler öffnen →“ zur eigenen Seite. */
    export let moreHref: string | undefined = undefined;
    export let moreText = 'Öffnen →';
    /** Notiz des Objekts (z. B. der Transaktion), sonst nur auf der eigenen Seite sichtbar. */
    export let notiz: string | null | undefined = undefined;
    /** Datei-Endpunkt des Objekts (`…/{id}/files`): zeigt die Zahl der Anhänge am Link. */
    export let attachmentsUrl: string | undefined = undefined;
    export let fetchImpl: typeof fetch = fetch;

    // Ohne Antwort (z. B. Dateiablage nicht erreichbar) erscheint einfach keine Zahl.
    let attachments = 0;
    onMount(async () => {
        if (!attachmentsUrl) return;
        try {
            const response = await walter_fetch(fetchImpl, attachmentsUrl);
            if (response.ok) {
                const dateien = (await response.json()) as unknown[];
                attachments = Array.isArray(dateien) ? dateien.length : 0;
            }
        } catch {
            attachments = 0;
        }
    });
</script>

{#if notiz?.trim()}
    <p class="notiz"><span class="notiz-titel">Notiz:</span> {notiz}</p>
{/if}

{#if items.length === 0}
    {#if leerText}
        <p class="leer">{leerText}</p>
    {/if}
{:else}
    <ul class="items">
        {#each items as item (item.id)}
            <li class="item">
                <div class="zeile">
                    <span class="datum">
                        {item.datum ? formatDate(item.datum) : ''}
                    </span>
                    <span class="betrag">
                        {item.betrag !== undefined
                            ? formatEuro(item.betrag)
                            : (item.wert ?? '')}
                    </span>
                    <span class="text">
                        {#if item.href}
                            <a href={item.href}>{item.text}</a>
                        {:else}
                            {item.text}
                        {/if}
                    </span>
                    <span class="tag">
                        {#if item.tag}
                            <Tag size="sm" type={item.tag.tag}
                                >{item.tag.text}</Tag
                            >
                        {/if}
                    </span>
                    <span class="aktion"><slot name="aktion" {item} /></span>
                </div>
                {#if item.parts && item.parts.length > 0}
                    <ul class="parts">
                        {#each item.parts as part}
                            <li class="zeile part" class:dimmed={part.dimmed}>
                                <span class="datum" />
                                <span class="betrag">
                                    {part.betrag !== undefined
                                        ? formatEuro(part.betrag)
                                        : ''}
                                </span>
                                <span class="text">
                                    {#if part.href}
                                        <a href={part.href}>{part.text}</a>
                                    {:else}
                                        {part.text}
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
{#if moreHref}
    <div class="mehr">
        <a href={moreHref}>{moreText}</a>
        {#if attachments > 0}
            <a class="attachments" href={moreHref}>
                <Attachment size={16} />
                {attachments === 1 ? '1 Anhang' : `${attachments} Anhänge`}
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
    .items {
        width: 100%;
    }
    .item {
        padding: 0.375rem 0;
        border-bottom: 1px solid var(--cds-ui-03);
    }
    .item:last-child {
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
    .part {
        min-height: 1.5rem;
        font-size: 0.875rem;
        color: var(--cds-text-02);
    }
    .part .text {
        padding-left: 1rem;
        border-left: 2px solid var(--cds-ui-03);
    }
    .dimmed {
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
    .attachments {
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
        .part {
            grid-template-columns: 5.5rem minmax(0, 1fr);
            grid-template-areas: 'betrag text';
        }
        .part .datum {
            display: none;
        }
    }
</style>
