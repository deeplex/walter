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
    import { createEventDispatcher } from 'svelte';
    import {
        Button,
        InlineNotification,
        Row,
        Tile
    } from 'carbon-components-svelte';

    export let hatHinweise = false;
    export let fehlendeBuchungen = 0;
    export let inkonsistenteBuchungen = 0;
    export let abweichendeMieten = 0;
    export let warnungenZaehlerstaende: string[] = [];
    export let warnungenMieten: string[] = [];
    export let warnungenSonstige: string[] = [];
    export let selectedWohnungIdsFlat: number[] = [];
    export let downloadLoading = false;
    export let bookLoading = false;
    export let alleVertraegeGebuchtUndKorrekt = false;
    export let downloadModusText = 'ENTWURF';
    export let downloadFormat: 'pdf' | 'docx' = 'pdf';
    /** Keine Betriebskosten im Jahr erfasst → es gibt nichts zu buchen. */
    export let nichtsAbzurechnen = false;
    /** Harte Fehler (z.B. fehlender Zählerstand) → Buchen gesperrt. */
    export let fehler: string[] = [];

    const dispatch = createEventDispatcher<{
        download: undefined;
        book: undefined;
    }>();

    const onDownload = () => dispatch('download');
    const onBook = () => dispatch('book');
</script>

<Tile>
    <h4>Hinweise</h4>

    {#if fehler.length > 0}
        <InlineNotification
            kind="error"
            lowContrast
            hideCloseButton
            title="Abrechnung nicht buchbar:"
            style="margin-bottom: 1rem;"
        >
            <div slot="subtitle">
                <ul style="margin: 0.25rem 0 0 1.1rem;">
                    {#each fehler as f}
                        <li style="margin: 0.15rem 0;">{f.replace(/^.*?Fehler:\s*/, '')}</li>
                    {/each}
                </ul>
            </div>
        </InlineNotification>
    {/if}

    {#if nichtsAbzurechnen}
        <InlineNotification
            kind="info"
            lowContrast
            hideCloseButton
            title="Nichts abzurechnen:"
            subtitle="Für dieses Jahr sind keine Betriebskosten erfasst — es gibt nichts zu buchen. Falls für diesen Zeitraum bewusst nicht abgerechnet wird, kannst du in der Jahresabschlusskontrolle einen Abrechnungsverzicht setzen."
            style="margin-bottom: 1rem;"
        />
    {/if}

    {#if hatHinweise}
        <p style="margin: 0.2rem 0; color: var(--cds-text-primary);">
            Noch ungebucht: {fehlendeBuchungen}
        </p>
        <p style="margin: 0.2rem 0; color: var(--cds-text-primary);">
            Abweichende Buchungen: {inkonsistenteBuchungen}
        </p>
        <p style="margin: 0.2rem 0; color: var(--cds-text-primary);">
            Abweichende Mieten: {abweichendeMieten}
        </p>

        {#if warnungenZaehlerstaende.length > 0}
            <p style="margin: 0.2rem 0; color: var(--cds-text-primary);">
                Zählerstände: {warnungenZaehlerstaende.length} Hinweis(e)
            </p>
        {/if}

        {#if warnungenMieten.length > 0}
            <p style="margin: 0.2rem 0; color: var(--cds-text-primary);">
                Mieten: {warnungenMieten.length} Hinweis(e)
            </p>
        {/if}

        {#if warnungenSonstige.length > 0}
            <p
                style="margin: 0.5rem 0 0.2rem; color: var(--cds-text-secondary);"
            >
                Weitere Hinweise:
            </p>
            <ul
                style="margin: 0.1rem 0 0 1.25rem; color: var(--cds-text-secondary);"
            >
                {#each warnungenSonstige as warnung}
                    <li style="margin: 0.15rem 0;">{warnung}</li>
                {/each}
            </ul>
        {/if}
    {:else}
        <p
            style="margin: 0; color: var(--cds-support-success); font-weight: 600;"
        >
            Keine offenen Buchungen, keine Abweichungen, keine Mietabweichungen,
            keine Warnungen.
        </p>
    {/if}

    <Row
        style="margin: 1.5rem 0 2rem; gap: 1rem; align-items: flex-end; justify-content: flex-end; flex-wrap: wrap;"
    >
        <div style="min-width: 12rem;">
            <label
                for="download-format"
                style="display: block; margin-bottom: 0.35rem; font-size: 0.875rem; color: var(--cds-text-secondary);"
            >
                Download-Format
            </label>
            <select
                id="download-format"
                bind:value={downloadFormat}
                style="height: 2.5rem; padding: 0 0.75rem; border: 1px solid var(--cds-border-subtle); background: var(--cds-layer); color: var(--cds-text-primary); width: 100%;"
            >
                <option value="pdf">PDF (.pdf)</option>
                <option value="docx">Word (.docx)</option>
            </select>
        </div>

        <Button
            kind="secondary"
            disabled={selectedWohnungIdsFlat.length === 0 || downloadLoading}
            on:click={onDownload}
            title={`Downloadstatus: ${downloadModusText}`}
        >
            {downloadLoading
                ? 'Abrechnungen werden erstellt…'
                : alleVertraegeGebuchtUndKorrekt
                  ? 'Abrechnungen runterladen'
                  : 'Entwürfe runterladen'}
        </Button>

        <Button
            kind="danger"
            disabled={selectedWohnungIdsFlat.length === 0 ||
                bookLoading ||
                nichtsAbzurechnen ||
                fehler.length > 0}
            on:click={onBook}
            title={fehler.length > 0
                ? 'Fehler in der Abrechnung — Buchen gesperrt.'
                : nichtsAbzurechnen
                  ? 'Keine Betriebskosten erfasst — nichts zu buchen.'
                  : undefined}
        >
            Buchungen erstellen
        </Button>
    </Row>
</Tile>
