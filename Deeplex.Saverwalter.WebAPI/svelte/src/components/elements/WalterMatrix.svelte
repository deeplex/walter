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

<script lang="ts">
    import {
        filtereMatrix,
        zaehleOffeneInGruppe,
        type Matrix,
        type MatrixZelle
    } from '$walter/services/matrix';

    export let matrix: Matrix;
    export let zeilenLabel = 'Wohnung';
    export let onZelle: (
        zelle: MatrixZelle,
        zeileId: string,
        spaltenId: string
    ) => void;

    /** Suchtext und „Nur Offene“ gehören der Seite (siehe WalterMatrixWerkzeuge). */
    export let suche = '';
    export let nurOffene = false;
    /** Zugeklappte Gruppen (Adressen); beim Suchen sind alle Treffer sichtbar. */
    let zugeklappt = new Set<string>();

    function umschalten(gruppe: string) {
        const neu = new Set(zugeklappt);
        if (neu.has(gruppe)) neu.delete(gruppe);
        else neu.add(gruppe);
        zugeklappt = neu;
    }

    // Beim ersten Laden eines Rasters (und bei einem anderen Jahr) starten
    // vollständig erledigte Adressen zugeklappt. Danach entscheidet nur noch der
    // Klick, damit eine Gruppe nicht unter den Händen zuklappt, wenn man gerade
    // ihr letztes offenes Feld erledigt hat.
    let geladenerSchluessel = '';
    $: {
        const schluessel = `${matrix.spalten.map((s) => s.id)}|${matrix.zeilen.map((z) => z.id)}`;
        if (schluessel !== geladenerSchluessel && matrix.zeilen.length > 0) {
            geladenerSchluessel = schluessel;
            const erledigt = new Set<string>();
            for (const zeile of matrix.zeilen) {
                if (
                    zeile.gruppe !== undefined &&
                    zaehleOffeneInGruppe(matrix, zeile.gruppe) === 0
                )
                    erledigt.add(zeile.gruppe);
            }
            zugeklappt = erledigt;
        }
    }

    $: istZu = (gruppe: string | undefined) =>
        gruppe !== undefined && suche.trim() === '' && zugeklappt.has(gruppe);

    $: ansicht = filtereMatrix(matrix, nurOffene, suche);
    $: sichtbar = ansicht.zeilen;
    $: spalten = ansicht.spalten;
    // Lange Spaltenüberschriften (z. B. Kostenarten) stehen schräg, damit die
    // Tabelle nicht breiter wird als nötig.
    $: laengsterKopf = Math.max(0, ...spalten.map((s) => s.label.length));
    $: schraeg = laengsterKopf > 6;
    $: kopfHoehe = Math.min(14, 2 + laengsterKopf * 0.42);
    $: leer = matrix.zeilen.length === 0;
    $: infoSpalten = matrix.infoSpalten ?? [zeilenLabel];
    $: spaltenAnzahl = infoSpalten.length + spalten.length;

    const symbol = {
        erledigt: '✓',
        teilweise: '◐',
        offen: '',
        ignoriert: ''
    } as const;
</script>

<div class="matrix">
    {#if leer}
        <p class="hinweis">Keine Einträge für dieses Jahr.</p>
    {:else}
        <div
            class="scroll"
            style={schraeg
                ? `padding-right: ${kopfHoehe * 0.75}rem`
                : undefined}
        >
            <table>
                <thead style={schraeg ? `height: ${kopfHoehe}rem` : undefined}>
                    <tr>
                        {#each infoSpalten as kopf}
                            <th>{kopf}</th>
                        {/each}
                        {#each spalten as sp (sp.id)}
                            {#if schraeg}
                                <th class="zelle schraeg" title={sp.label}>
                                    <div><span>{sp.label}</span></div>
                                </th>
                            {:else}
                                <th class="zelle">{sp.label}</th>
                            {/if}
                        {/each}
                    </tr>
                </thead>
                <tbody>
                    {#each sichtbar as zeile, index (zeile.id)}
                        {#if zeile.gruppe !== undefined && zeile.gruppe !== sichtbar[index - 1]?.gruppe}
                            {@const offen = zaehleOffeneInGruppe(
                                { ...matrix, spalten, zeilen: sichtbar },
                                zeile.gruppe
                            )}
                            <tr class="gruppe">
                                <th colspan={spaltenAnzahl}>
                                    <button
                                        type="button"
                                        class="gruppe-knopf"
                                        aria-expanded={!istZu(zeile.gruppe)}
                                        on:click={() =>
                                            umschalten(zeile.gruppe ?? '')}
                                    >
                                        <span
                                            class="pfeil"
                                            class:zu={istZu(zeile.gruppe)}
                                            >▾</span
                                        >
                                        {zeile.gruppe}
                                    </button>
                                    <span
                                        class="gruppe-status"
                                        class:fertig={offen === 0}
                                    >
                                        {offen === 0 ? '✓' : `${offen} offen`}
                                    </span>
                                </th>
                            </tr>
                        {/if}
                        <tr class:ausgeblendet={istZu(zeile.gruppe)}>
                            {#each zeile.info ?? [zeile.label] as wert, i}
                                {#if i === 0}
                                    <th scope="row">
                                        <span class="label" title={zeile.label}
                                            >{wert}</span
                                        >
                                    </th>
                                {:else}
                                    <td class="info">
                                        <span class="label" title={zeile.label}
                                            >{wert}</span
                                        >
                                    </td>
                                {/if}
                            {/each}
                            {#each spalten as s (s.id)}
                                {@const z = zeile.zellen[s.id]}
                                <td class="zelle">
                                    {#if z}
                                        <button
                                            type="button"
                                            class="feld {z.status}"
                                            title={z.titel}
                                            aria-label={`${zeile.label}, ${s.label}: ${z.status}`}
                                            on:click={() =>
                                                onZelle(z, zeile.id, s.id)}
                                        >
                                            {z.text ?? symbol[z.status]}
                                        </button>
                                    {:else}
                                        <span class="nicht-relevant">·</span>
                                    {/if}
                                </td>
                            {/each}
                        </tr>
                    {/each}
                </tbody>
            </table>
        </div>
        {#if sichtbar.length === 0}
            <p class="hinweis">Keine Treffer.</p>
        {/if}
    {/if}
</div>

<style>
    .scroll {
        overflow-x: auto;
    }

    table {
        border-collapse: separate;
        border-spacing: 2px;
        width: 100%;
    }

    th,
    td {
        padding: 0;
        vertical-align: middle;
    }

    thead th {
        text-align: left;
        font-weight: 600;
        padding: 0.25rem 0.5rem;
        white-space: nowrap;
    }

    tbody th {
        text-align: left;
        font-weight: 400;
        padding: 0 0.75rem 0 0.5rem;
        height: 1.75rem;
    }

    tr.gruppe th {
        text-align: left;
        font-weight: 600;
        padding: 0.5rem 0.5rem 0.15rem;
        border-bottom: 1px solid var(--cds-border-subtle, #c6c6c6);
    }

    .gruppe-status {
        margin-left: 0.75rem;
        font-weight: 400;
        font-size: 0.875rem;
        opacity: 0.7;
    }

    .gruppe-status.fertig {
        color: var(--cds-support-success, #24a148);
        opacity: 1;
    }

    tr.ausgeblendet {
        display: none;
    }

    .gruppe-knopf {
        all: unset;
        cursor: pointer;
        display: inline-flex;
        gap: 0.4rem;
        align-items: center;
    }

    .gruppe-knopf:focus-visible {
        outline: 2px solid var(--cds-focus, #0f62fe);
    }

    .pfeil {
        display: inline-block;
        transition: transform 0.1s;
    }

    .pfeil.zu {
        transform: rotate(-90deg);
    }

    td.info {
        padding: 0 0.75rem 0 0.5rem;
        height: 1.75rem;
        white-space: nowrap;
    }

    tbody th {
        font-size: 0.875rem;
    }

    .label {
        display: block;
        max-width: 22rem;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    thead th.schraeg {
        padding: 0;
        vertical-align: bottom;
        overflow: visible;
    }

    /* Die Überschrift steht um 45° gedreht und wächst nach rechts oben. */
    thead th.schraeg > div {
        width: 3.5rem;
        transform-origin: left bottom;
        transform: translateX(1.5rem) rotate(-45deg);
        white-space: nowrap;
    }

    thead th.schraeg > div > span {
        display: inline-block;
        padding: 0 0.25rem;
        border-bottom: 1px solid var(--cds-border-subtle, #c6c6c6);
    }

    thead th.zelle,
    td.zelle {
        text-align: center;
    }

    td.zelle {
        width: 3.5rem;
        min-width: 3rem;
    }

    .feld {
        display: block;
        width: 100%;
        height: 1.75rem;
        padding: 0;
        border: none;
        cursor: pointer;
        color: inherit;
        font: inherit;
        line-height: 1.75rem;
    }

    .feld.erledigt {
        background: var(--cds-support-success, #24a148);
        color: #fff;
    }

    .feld.teilweise {
        background: var(--cds-support-warning, #f1c21b);
        color: #161616;
    }

    .feld.offen {
        background: var(--cds-layer-accent, #e0e0e0);
        outline: 1px dashed var(--cds-border-strong, #8d8d8d);
        outline-offset: -2px;
    }

    .feld.ignoriert {
        background: transparent;
        outline: 1px dotted var(--cds-border-subtle, #c6c6c6);
        outline-offset: -2px;
        opacity: 0.35;
    }

    .feld:hover,
    .feld:focus-visible {
        filter: brightness(0.9);
    }

    .nicht-relevant {
        display: block;
        height: 1.75rem;
        line-height: 1.75rem;
        opacity: 0.3;
    }

    .hinweis {
        opacity: 0.7;
        margin: 0.5rem 0;
    }
</style>
