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
    import type { PageData } from './$types';
    import { WalterSelectionEntry, WalterToastContent } from '$walter/lib';
    import {
        WalterHeader,
        WalterGrid,
        WalterMultiSelect,
        WalterAbrechnungslaufGruppe
    } from '$walter/components';
    import WalterAbrechnungslaufHinweiseAktionen from '$walter/components/abrechnung/abrechnungslauf/WalterAbrechnungslaufHinweiseAktionen.svelte';
    import {
        Button,
        InlineNotification,
        NumberInput,
        Row
    } from 'carbon-components-svelte';
    import { tick } from 'svelte';
    import { download_file_blob } from '$walter/services/files';
    import { walter_fetch, walter_post } from '$walter/services/requests';
    import { addToast } from '$walter/store';
    import type {
        AbrechnungsresultatInfo,
        AbrechnungslaufGruppeResult
    } from '$walter/components/abrechnung/abrechnungslauf/AbrechnungslaufTypes';

    export let data: PageData;

    type AbrechnungsGruppe = {
        groupKey: string;
        wohnungIds: number[];
        bezeichnung: string;
    };

    type AbrechnungslaufResult = {
        gruppen: AbrechnungslaufGruppeResult[];
        warnungen: string[];
    };

    const ensureBatchResult = (value: unknown): AbrechnungslaufResult[] => {
        if (!Array.isArray(value)) {
            throw new Error(
                'Unerwartete Serverantwort: Liste von Gruppen-Resultaten erwartet.'
            );
        }

        return value as AbrechnungslaufResult[];
    };

    const mergeBatchResults = (
        batchResults: AbrechnungslaufResult[]
    ): AbrechnungslaufResult => ({
        gruppen: batchResults.flatMap((item) => item.gruppen),
        warnungen: batchResults.flatMap((item) => item.warnungen)
    });
    // ── Hilfsfunktionen ──────────────────────────────────────────────────

    let abrechnungsGruppen: AbrechnungsGruppe[] = [];

    async function loadAbrechnungsGruppen() {
        const resp = await walter_fetch(
            data.fetchImpl,
            '/api/abrechnungslauf/gruppen'
        );
        if (!resp.ok) throw new Error(await resp.text());

        abrechnungsGruppen = await resp.json();
        return abrechnungsGruppen;
    }

    const _urlParams =
        typeof window !== 'undefined'
            ? new URLSearchParams(window.location.search)
            : new URLSearchParams();
    const _initialGruppenKeys =
        _urlParams.get('gruppen')?.split(',').filter(Boolean) ?? [];

    const gruppenEntries = loadAbrechnungsGruppen().then((gruppen) => {
        const entries = gruppen.map(
            (gruppe) =>
                new WalterSelectionEntry(gruppe.groupKey, gruppe.bezeichnung)
        );
        if (_initialGruppenKeys.length > 0) {
            selectedGruppen = entries.filter((e) =>
                _initialGruppenKeys.includes(String(e.id))
            );
            if (selectedGruppen.length > 0) {
                tick().then(preview);
            }
        }
        return entries;
    });

    let jahr =
        parseInt(_urlParams.get('jahr') ?? '') || new Date().getFullYear() - 1;
    let selectedGruppen: WalterSelectionEntry[] | undefined;

    $: selectedGroupKeys = selectedGruppen?.map((e) => String(e.id)) ?? [];
    $: if (typeof window !== 'undefined') {
        const params = new URLSearchParams(window.location.search);
        params.set('jahr', String(jahr));
        if (selectedGroupKeys.length > 0) {
            params.set('gruppen', selectedGroupKeys.join(','));
        } else {
            params.delete('gruppen');
        }
        history.replaceState(history.state, '', `?${params}`);
    }
    $: selectedGruppenWohnungIds = abrechnungsGruppen
        .filter((gruppe) => selectedGroupKeys.includes(gruppe.groupKey))
        .map((gruppe) => gruppe.wohnungIds);

    let result: AbrechnungslaufResult | null = null;
    let resultError: string | null = null;
    let loading = false;

    let bookLoading = false;
    let downloadLoading = false;
    let downloadFormat: 'pdf' | 'docx' = 'pdf';

    const BookToast = new WalterToastContent(
        'Buchungen erfolgreich erstellt',
        'Buchungen konnten nicht erstellt werden',
        (vertraege: unknown) =>
            `Die Buchung ist auf ${Number(vertraege ?? 0)} Verträgen erfolgt.`,
        (error: unknown) =>
            typeof error === 'string' && error.length > 0
                ? error
                : 'Bitte erneut versuchen.'
    );

    const DownloadToast = new WalterToastContent(
        'Abrechnungen heruntergeladen',
        'Download fehlgeschlagen',
        (fileName: unknown) =>
            typeof fileName === 'string' && fileName.length > 0
                ? `Datei ${fileName} wurde heruntergeladen.`
                : 'Abrechnungen wurden heruntergeladen.',
        (error: unknown) =>
            typeof error === 'string' && error.length > 0
                ? error
                : 'Bitte erneut versuchen.'
    );

    async function preview() {
        if (selectedGruppenWohnungIds.length === 0) return;
        loading = true;
        result = null;
        resultError = null;
        try {
            const resp = await walter_fetch(
                data.fetchImpl,
                '/api/abrechnungslauf/preview',
                {
                    method: 'POST',
                    body: JSON.stringify({
                        gruppen: selectedGruppenWohnungIds.map(
                            (wohnungIds) => ({
                                wohnungIds
                            })
                        ),
                        jahr
                    })
                }
            );
            if (resp.ok) {
                const batchResult = ensureBatchResult(await resp.json());
                result = mergeBatchResults(batchResult);
            } else {
                resultError = await resp.text();
            }
        } catch (e) {
            resultError = String(e);
        } finally {
            loading = false;
        }
    }

    async function book() {
        if (selectedGruppenWohnungIds.length === 0) return;
        bookLoading = true;
        try {
            const resp = await walter_fetch(
                data.fetchImpl,
                '/api/abrechnungslauf/book',
                {
                    method: 'POST',
                    body: JSON.stringify({
                        gruppen: selectedGruppenWohnungIds.map(
                            (wohnungIds) => ({
                                wohnungIds
                            })
                        ),
                        jahr
                    })
                }
            );
            if (resp.ok) {
                const batchResult = ensureBatchResult(await resp.json());
                result = mergeBatchResults(batchResult);
                const vertragCount = result.gruppen.reduce(
                    (sum, gruppe) => sum + gruppe.resultate.length,
                    0
                );
                addToast(BookToast, true, vertragCount);
                await preview();
            } else {
                addToast(BookToast, false, await resp.text());
            }
        } catch (e) {
            addToast(BookToast, false, String(e));
        } finally {
            bookLoading = false;
        }
    }

    $: selectedWohnungIdsFlat = [...new Set(selectedGruppenWohnungIds.flat())];
    $: vertragResultate = result
        ? result.gruppen
              .flatMap((g) => g.resultate)
              .filter((r) => r.vertragId != null)
        : [];
    $: fehlendeBuchungen = vertragResultate.filter(
        (r) => r.gebuchtesAbrechnungsResultat == null
    ).length;
    $: inkonsistenteBuchungen = vertragResultate.filter(
        (r) =>
            r.gebuchtesAbrechnungsResultat != null &&
            Math.abs(r.gebuchtesAbrechnungsResultat - r.rechnungsbetrag) > 0.005
    ).length;
    $: abweichendeMieten = vertragResultate.filter(
        (r) => Math.abs(r.mietSaldo) > 0.005
    ).length;
    $: warnungen = result?.warnungen ?? [];
    $: warnungenZaehlerstaende = warnungen.filter((w) =>
        /z(a|ä)hler|zaehlerstand/i.test(w)
    );
    $: warnungenMieten = warnungen.filter((w) => /miet|kaltmiete/i.test(w));
    $: warnungenSonstige = warnungen.filter(
        (w) => !/z(a|ä)hler|zaehlerstand/i.test(w) && !/miet|kaltmiete/i.test(w)
    );
    $: hatHinweise =
        fehlendeBuchungen > 0 ||
        inkonsistenteBuchungen > 0 ||
        abweichendeMieten > 0 ||
        warnungen.length > 0;
    $: alleVertraegeGebuchtUndKorrekt =
        vertragResultate.length > 0 &&
        fehlendeBuchungen === 0 &&
        inkonsistenteBuchungen === 0;
    $: downloadModusText = alleVertraegeGebuchtUndKorrekt
        ? 'Final ohne ENTWURF'
        : 'ENTWURF';

    function setDraftSuffix(fileName: string, useDraftSuffix: boolean) {
        const lastDot = fileName.lastIndexOf('.');
        const base = lastDot >= 0 ? fileName.slice(0, lastDot) : fileName;
        const ext = lastDot >= 0 ? fileName.slice(lastDot) : '';
        const cleanBase = base.endsWith('_ENTWURF')
            ? base.slice(0, -'_ENTWURF'.length)
            : base;
        return useDraftSuffix
            ? `${cleanBase}_ENTWURF${ext}`
            : `${cleanBase}${ext}`;
    }

    async function downloadAbrechnungen() {
        if (selectedWohnungIdsFlat.length === 0) return;
        downloadLoading = true;
        try {
            const response = await walter_post(
                `/api/abrechnungslauf/print/${downloadFormat}`,
                {
                    wohnungIds: selectedWohnungIdsFlat,
                    jahr
                }
            );
            if (!response.ok) {
                addToast(DownloadToast, false, await response.text());
                return;
            }

            const disposition =
                response.headers.get('content-disposition') ?? '';
            const match = disposition.match(
                /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/
            );
            const contentType = response.headers.get('content-type') ?? '';
            const fallbackExt = contentType.includes('zip')
                ? 'zip'
                : downloadFormat;
            const fallbackFileName = `NK_${jahr}_Abrechnung.${fallbackExt}`;
            const serverFileName = match
                ? match[1].replace(/['"]/g, '')
                : fallbackFileName;
            const fileName = setDraftSuffix(
                serverFileName,
                !alleVertraegeGebuchtUndKorrekt
            );

            download_file_blob(await response.blob(), fileName);
            addToast(DownloadToast, true, fileName);
        } catch (e) {
            addToast(DownloadToast, false, String(e));
        } finally {
            downloadLoading = false;
        }
    }
</script>

<WalterHeader title="Abrechnungslauf" />

<WalterGrid>
    <Row
        style="margin-bottom: 1rem; gap: 1rem; align-items: flex-end; flex-wrap: wrap;"
    >
        <div style="flex: 1 1 20rem;">
            <WalterMultiSelect
                titleText="Abrechnungsgruppen"
                bind:value={selectedGruppen}
                entries={gruppenEntries}
            />
        </div>

        <div style="width: 10rem;">
            <NumberInput
                label="Jahr"
                bind:value={jahr}
                min={2000}
                max={2099}
                hideSteppers={false}
            />
        </div>

        <div style="padding-bottom: 0.75rem;">
            <Button
                disabled={selectedGruppenWohnungIds.length === 0 || loading}
                on:click={preview}
            >
                Vorschau zeigen
            </Button>
        </div>
    </Row>

    {#if loading}
        <p style="color: var(--cds-text-secondary);">Wird berechnet…</p>
    {/if}

    {#if resultError}
        <InlineNotification
            kind="error"
            title="Fehler:"
            subtitle={resultError}
            lowContrast
        />
    {/if}

    {#if result}
        <p style="margin-bottom: 1rem; color: var(--cds-text-secondary);">
            {result.gruppen
                .flatMap((g) => g.resultate)
                .filter((r) => r.gebuchtesAbrechnungsResultat == null).length} neu
            · {result.gruppen
                .flatMap((g) => g.resultate)
                .filter((r) => r.gebuchtesAbrechnungsResultat != null).length}
            bereits gebucht
            {#if result.warnungen.length > 0}· {result.warnungen.length} Warnungen{/if}
        </p>

        {#each result.gruppen as gruppe}
            <WalterAbrechnungslaufGruppe
                {gruppe}
                {jahr}
                fetchImpl={data.fetchImpl}
            />
        {/each}

        <WalterAbrechnungslaufHinweiseAktionen
            {hatHinweise}
            {fehlendeBuchungen}
            {inkonsistenteBuchungen}
            {abweichendeMieten}
            {warnungenZaehlerstaende}
            {warnungenMieten}
            {warnungenSonstige}
            {selectedWohnungIdsFlat}
            {downloadLoading}
            {bookLoading}
            {alleVertraegeGebuchtUndKorrekt}
            {downloadModusText}
            bind:downloadFormat
            on:download={downloadAbrechnungen}
            on:book={book}
        />
    {/if}
</WalterGrid>
