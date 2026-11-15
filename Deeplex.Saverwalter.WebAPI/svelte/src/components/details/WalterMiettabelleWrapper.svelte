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
    import { entityRefHref } from '$walter/lib/WalterEntityRef';
    import {
        WalterUmlageEntry,
        WalterZaehlerstandEntry,
        type WalterUmlageEntry as WalterUmlageEntryType,
        type WalterZahlungsZiel
    } from '$walter/lib';
    import {
        Tab,
        TabContent,
        Tabs,
        NumberInput,
        SkeletonText,
        Tile
    } from 'carbon-components-svelte';
    import { WalterDataTable } from '$walter/components';
    import WalterZahlungErfassen from './WalterZahlungErfassen.svelte';
    import { convertDateCanadian } from '$walter/services/utils';
    import { walter_get } from '$walter/services/requests';
    import WalterZaehlerstand from './WalterZaehlerstand.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import { onMount } from 'svelte';
    import { goto } from '$app/navigation';
    import WalterMatrix from '../elements/WalterMatrix.svelte';
    import WalterMatrixWerkzeuge from '../elements/WalterMatrixWerkzeuge.svelte';
    import WalterMatrixDetailInhalt from '../elements/WalterMatrixDetailInhalt.svelte';
    import {
        baueRechnungsMatrix,
        baueAbrechnungsMatrix,
        baueGarageMatrix,
        baueMietMatrix,
        baueZaehlerMatrix,
        zaehlerDetail,
        leeresDetail,
        mietDetail,
        rechnungsDetail,
        zaehleOffen,
        zaehleOffeneBezuege,
        type MatrixDetail,
        type AbrechnungsMatrixZeileDaten,
        type GarageMatrixZeileDaten,
        type MietkontoDaten,
        type MietMatrixZeileDaten,
        type RechnungDaten,
        type ZaehlerMatrixZeileDaten,
        type MatrixZelle
    } from '$walter/services/matrix';
    import {
        euro,
        gruppiereNachId,
        plural,
        zeitraumText
    } from '$walter/services/postings';

    interface OffeneBkForderungEntry {
        buchungssatzId: string;
        bezeichnung: string;
        betrag: number;
        schonGezahlt: number;
        verbleibend: number;
    }

    interface GuthabenEntry {
        vertragId: number;
        vertragBezeichnung: string;
        monat: string;
        guthaben: number;
    }

    export let fetchImpl: typeof fetch;

    let umlagen: WalterUmlageEntryType[] = [];
    let umlagenReady = false;

    // ── Mietmatrix (Vertrag × Monat) ────────────────────────────────────────
    let mietMatrixDaten: MietMatrixZeileDaten[] = [];
    let mietMatrixReady = false;

    async function ladeMietMatrix(jahr: number) {
        mietMatrixReady = false;
        try {
            const resp = await walter_get(
                `/api/offene-forderungen/matrix/${jahr}`,
                fetchImpl
            );
            mietMatrixDaten = (resp as MietMatrixZeileDaten[]) ?? [];
        } catch (e) {
            console.error('Konnte die Mietmatrix nicht laden:', e);
            mietMatrixDaten = [];
        }
        mietMatrixReady = true;
    }

    // ── Garagen (Matrix Garage × Monat, eigenständige Garagenverträge) ──────
    let garageMatrixDaten: GarageMatrixZeileDaten[] = [];
    let garageMatrixReady = false;

    async function ladeGarageMatrix(jahr: number) {
        garageMatrixReady = false;
        try {
            const resp = await walter_get(
                `/api/offene-forderungen/matrix/garagen/${jahr}`,
                fetchImpl
            );
            garageMatrixDaten = (resp as GarageMatrixZeileDaten[]) ?? [];
        } catch (e) {
            console.error('Konnte die Garagenmatrix nicht laden:', e);
            garageMatrixDaten = [];
        }
        garageMatrixReady = true;
    }

    // ── Miet-Guthaben (Überzahlungen) ──────────────────────────────────────
    let guthaben: GuthabenEntry[] = [];
    let guthabenReady = false;

    async function ladeGuthaben(jahr: number) {
        guthabenReady = false;
        try {
            const resp = await walter_get(
                `/api/offene-forderungen/guthaben/${jahr}`,
                fetchImpl
            );
            guthaben = (resp as GuthabenEntry[]) ?? [];
        } catch (e) {
            console.error('Konnte Guthaben nicht laden:', e);
            guthaben = [];
        }
        guthabenReady = true;
    }

    // ── Offene BK-Forderungen ───────────────────────────────────────────────
    let offeneBkForderungen: OffeneBkForderungEntry[] = [];
    let bkForderungenReady = false;

    async function ladeOffeneBkForderungen(jahr: number) {
        bkForderungenReady = false;
        try {
            const resp = await walter_get(
                `/api/offene-forderungen/bk/${jahr}`,
                fetchImpl
            );
            offeneBkForderungen = (resp as OffeneBkForderungEntry[]) ?? [];
        } catch (e) {
            console.error('Konnte offene BK-Forderungen nicht laden:', e);
            offeneBkForderungen = [];
        }
        bkForderungenReady = true;
    }

    // ── Zähler (Zähler × Jahr) ──────────────────────────────────────────────
    let zaehlerMatrixDaten: ZaehlerMatrixZeileDaten[] = [];
    let zaehlerMatrixReady = false;

    async function ladeZaehlerMatrix(jahr: number) {
        zaehlerMatrixReady = false;
        try {
            const resp = await walter_get(
                `/api/offene-forderungen/matrix/zaehler/${jahr}`,
                fetchImpl
            );
            zaehlerMatrixDaten = (resp as ZaehlerMatrixZeileDaten[]) ?? [];
        } catch (e) {
            console.error('Konnte die Zählermatrix nicht laden:', e);
            zaehlerMatrixDaten = [];
        }
        zaehlerMatrixReady = true;
    }

    // ── Wohnungen mit nötiger Abrechnung im Jahr ────────────────────────────
    // Für die übrigen Wohnungen sind Rechnungen und Zählerstände nicht nötig; ist die
    // Abrechnung schon gesendet, gelten sie als erledigt.
    let benoetigteWohnungen: Map<number, boolean> | undefined;

    async function ladeBenoetigteWohnungen(jahr: number) {
        try {
            const resp = await walter_get(
                `/api/fristen/benoetigt/${jahr}`,
                fetchImpl
            );
            if (jahr === selectedYear)
                benoetigteWohnungen = new Map(
                    (
                        (resp as {
                            wohnungId: number;
                            abgeschlossen: boolean;
                        }[]) ?? []
                    ).map((w) => [w.wohnungId, w.abgeschlossen])
                );
        } catch (e) {
            console.error('Konnte die nötigen Abrechnungen nicht laden:', e);
            benoetigteWohnungen = undefined;
        }
    }

    // ── Abrechnungen (Matrix Vertrag × Abrechnungsjahr) ─────────────────────
    let abrechnungsDaten: AbrechnungsMatrixZeileDaten[] = [];
    let abrechnungenReady = false;

    async function ladeAbrechnungen() {
        try {
            const resp = await walter_get('/api/fristen/matrix', fetchImpl);
            abrechnungsDaten = (resp as AbrechnungsMatrixZeileDaten[]) ?? [];
        } catch (e) {
            console.error('Konnte die Abrechnungsmatrix nicht laden:', e);
            abrechnungsDaten = [];
        }
        abrechnungenReady = true;
    }

    onMount(async () => {
        ladeAbrechnungen();
        ladeBenoetigteWohnungen(selectedYear);
        await Promise.all([
            ladeMietMatrix(selectedYear),
            ladeGarageMatrix(selectedYear),
            ladeOffeneBkForderungen(selectedYear),
            ladeGuthaben(selectedYear),
            ladeZaehlerMatrix(selectedYear)
        ]);
        await ladeUmlagen();
    });

    // ── Year selector with URL persistence ────────────────────────────────
    const currentYear = new Date().getFullYear();
    const _urlParams =
        typeof window !== 'undefined'
            ? new URLSearchParams(window.location.search)
            : new URLSearchParams();
    let selectedYear = parseInt(_urlParams.get('jahr') ?? '') || currentYear;

    $: if (typeof window !== 'undefined') {
        const params = new URLSearchParams(window.location.search);
        params.set('jahr', String(selectedYear));
        history.replaceState(history.state, '', `?${params}`);
    }

    // ── Tabs, mit Auswahl in der URL ────────────────────────────────────────
    const tabNamen = [
        'mieten',
        'garagen',
        'rechnungen',
        'zaehler',
        'abrechnungen'
    ];
    let tab = Math.max(0, tabNamen.indexOf(_urlParams.get('tab') ?? ''));

    // Suche und „Nur Offene“ gelten für alle Tabs und stehen in der URL.
    let suche = _urlParams.get('q') ?? '';
    let nurOffene = _urlParams.get('offen') === '1';

    $: if (typeof window !== 'undefined') {
        const params = new URLSearchParams(window.location.search);
        params.set('tab', tabNamen[tab] ?? tabNamen[0]);
        if (suche.trim() !== '') params.set('q', suche);
        else params.delete('q');
        if (nurOffene) params.set('offen', '1');
        else params.delete('offen');
        history.replaceState(history.state, '', `?${params}`);
    }

    let _lastLoadedYear = selectedYear;
    $: if (selectedYear !== _lastLoadedYear) {
        _lastLoadedYear = selectedYear;
        ladeMietMatrix(selectedYear);
        ladeGarageMatrix(selectedYear);
        ladeOffeneBkForderungen(selectedYear);
        ladeGuthaben(selectedYear);
        ladeZaehlerMatrix(selectedYear);
        ladeBenoetigteWohnungen(selectedYear);
    }

    // ── Zahlung erfassen (für alle Forderungen) ─────────────────────────────
    let zahlungOffen = false;
    let zahlungZiel: WalterZahlungsZiel | undefined;
    let zahlungTitel = '';
    let zahlungVorschlag = 0;
    let zahlungEmpfaenger: number | undefined;

    function zahlungErfassen(
        ziel: WalterZahlungsZiel,
        titel: string,
        vorschlag: number,
        empfaenger?: number
    ) {
        zahlungDetail = undefined;
        zahlungZiel = ziel;
        zahlungTitel = titel;
        zahlungVorschlag = vorschlag;
        zahlungEmpfaenger = empfaenger;
        zahlungOffen = true;
    }

    async function nachZahlung() {
        switch (zahlungZiel?.art) {
            case 'miete':
                await Promise.all([
                    ladeMietMatrix(selectedYear),
                    ladeGuthaben(selectedYear)
                ]);
                break;
            case 'garage':
                await ladeGarageMatrix(selectedYear);
                break;
            case 'bkRechnung':
                await ladeOffeneBkForderungen(selectedYear);
                break;
            case 'bkNeu':
                alleRechnungen = undefined;
                await ladeUmlagen();
                break;
        }
    }

    const zahlungsButton = (onClick: () => void) => ({
        onClick: (e: MouseEvent) => {
            e.stopPropagation();
            onClick();
        },
        iconDescription: 'Zahlung erfassen'
    });

    // Mieten, Garagenmieten und Guthaben werden pro Vertrag zusammengefasst.
    // Einzelne Zahlungen werden im Mietkonto des Vertrags erfasst.
    const gruppenHeaders = (label: string, summeLabel: string) => [
        { key: 'bezeichnung', value: label },
        { key: 'monate', value: 'Monate' },
        { key: 'zeitraum', value: 'Zeitraum' },
        { key: 'summe', value: summeLabel }
    ];

    const guthabenHeaders = gruppenHeaders('Vertrag', 'Guthaben');

    $: guthabenGruppen = gruppiereNachId(
        guthaben.map((g) => ({
            id: g.vertragId,
            bezeichnung: g.vertragBezeichnung,
            monat: g.monat,
            betrag: g.guthaben
        }))
    );

    const gruppenRows = (
        gruppen: ReturnType<typeof gruppiereNachId>,
        prefix: string,
        href: (id: number) => string | undefined
    ) =>
        gruppen.map((g) => ({
            id: `${prefix}-${g.id}`,
            href: href(g.id),
            bezeichnung: g.bezeichnung,
            monate: g.anzahl,
            zeitraum: zeitraumText(g),
            summe: euro(g.summe)
        }));

    const vertragMietkontoHref = (id: number) =>
        entityRefHref({ type: 'Vertrag', id, section: 'mietkonto' });

    $: guthabenTableRows = gruppenRows(
        guthabenGruppen,
        'guthaben',
        vertragMietkontoHref
    );

    const summe = (gruppen: { summe: number }[]) =>
        gruppen.reduce((acc, g) => acc + g.summe, 0);

    // ── Mieten (Matrix Vertrag × Monat) ─────────────────────────────────────
    $: mietMatrix = baueMietMatrix(mietMatrixDaten);
    $: vertraegeMitOffenerMiete = mietMatrixDaten.filter((z) =>
        z.monate.some((m) => m.offen > 0.005)
    ).length;

    // ── Vorhandenes im Erfassen-Dialog ──────────────────────────────────────
    // Der Dialog zeigt über dem Formular, was für das Feld schon eingetragen ist.
    let zahlungDetail: MatrixDetail | undefined;
    let zahlungDetailLaden = false;
    let zahlungDetailLeerText = '';
    let zahlungFormularZu = false;
    let zahlungWeitereAktion = 'Weitere Zahlung erfassen';

    function mitDetail(
        leerText: string,
        formularZu: boolean,
        weitereAktion = 'Weitere Zahlung erfassen'
    ) {
        zahlungDetail = leeresDetail();
        zahlungDetailLaden = true;
        zahlungDetailLeerText = leerText;
        zahlungFormularZu = formularZu;
        zahlungWeitereAktion = weitereAktion;
    }

    async function mieteOeffnen(
        zelle: MatrixZelle,
        zeileId: string,
        spaltenId: string
    ) {
        const zeile = mietMatrixDaten.find((z) => `${z.vertragId}` === zeileId);
        const monat = zeile?.monate.find((m) => m.monat === zelle.ref);
        if (!zeile || !monat) return;

        mieteErfassen(zelle, zeileId, spaltenId);
        mitDetail(
            monat.soll <= 0.005
                ? 'Für diesen Monat ist keine Miete fällig.'
                : monat.offen <= 0.005
                  ? 'Für diesen Monat ist nichts offen.'
                  : 'Für diesen Monat ist noch keine Zahlung eingetragen.',
            monat.offen <= 0.005
        );

        let konto: MietkontoDaten | undefined;
        try {
            konto = (await walter_get(
                `/api/vertraege/${zeile.vertragId}/mietkonto`,
                fetchImpl
            )) as MietkontoDaten;
        } catch (e) {
            console.error('Konnte das Mietkonto nicht laden:', e);
        }
        zahlungDetail = mietDetail(
            monat,
            konto,
            vertragMietkontoHref(zeile.vertragId)
        );
        zahlungDetailLaden = false;
    }

    function mieteErfassen(
        zelle: MatrixZelle,
        zeileId: string,
        _spaltenId: string
    ) {
        const zeile = mietMatrixDaten.find((z) => `${z.vertragId}` === zeileId);
        const monat = zeile?.monate.find((m) => m.monat === zelle.ref);
        if (!zeile || !monat) return;
        const monatText = new Date(
            monat.monat + 'T00:00:00'
        ).toLocaleDateString('de-DE', { month: 'long', year: 'numeric' });
        zahlungErfassen(
            { art: 'miete', vertragId: zeile.vertragId, monat: monat.monat },
            `${zeile.vertragBezeichnung} – ${monatText}`,
            Math.max(monat.offen, 0),
            zeile.zahlungsempfaengerId
        );
    }

    // ── Abrechnungen (Matrix) ───────────────────────────────────────────────
    // Abgerechnet wird ein Jahr nach dem anderen; die letzte Spalte ist das
    // jüngste Jahr, das schon abzurechnen ist.
    $: abrechnungsMatrix = baueAbrechnungsMatrix(
        abrechnungsDaten,
        Math.min(selectedYear, currentYear - 1)
    );
    $: abrechnungenOffen = zaehleOffen(abrechnungsMatrix);

    // Die Abrechnung selbst entsteht im Abrechnungslauf.
    function abrechnungOeffnen(zelle: MatrixZelle) {
        goto(`/abrechnungslauf?jahr=${zelle.ref}`);
    }

    // ── Zähler (Matrix) ─────────────────────────────────────────────────────
    $: zaehlerMatrix = baueZaehlerMatrix(
        zaehlerMatrixDaten,
        selectedYear,
        benoetigteWohnungen
    );
    $: zaehlerOffen = zaehleOffen(zaehlerMatrix);

    let addZaehlerstandOpen = false;
    let addZaehlerstandEntry: Partial<WalterZaehlerstandEntry> = {};
    let zaehlerDetailDaten: MatrixDetail = leeresDetail();

    // Ein Klick zeigt zuerst die Stände des Jahres und bietet darunter das
    // Erfassen eines weiteren Standes an.
    function zaehlerOeffnen(_zelle: MatrixZelle, zeileId: string) {
        const z = zaehlerMatrixDaten.find((x) => `${x.id}` === zeileId);
        if (!z) return;
        zaehlerDetailDaten = zaehlerDetail(z, `/zaehler/${z.id}`);
        addZaehlerstandEntry = {
            datum:
                selectedYear === currentYear
                    ? convertDateCanadian(new Date())
                    : convertDateCanadian(new Date(selectedYear, 11, 31)),
            stand: z.lastStand,
            einheit: z.lastEinheit,
            zaehler: { id: `${z.id}`, text: z.kennnummer }
        };
        addZaehlerstandOpen = true;
    }

    async function onSubmitZaehlerstand() {
        addZaehlerstandOpen = false;
        addZaehlerstandEntry = {};
        await ladeZaehlerMatrix(selectedYear);
    }

    // ── Rechnungen (Matrix Wohnung × Umlage) ───────────────────────────────
    async function ladeUmlagen() {
        try {
            const paged =
                await WalterUmlageEntry.GetPaged<WalterUmlageEntryType>(
                    fetchImpl,
                    { take: 10000 }
                );
            umlagen = paged.items;
        } catch (e) {
            console.error('Konnte Umlagen nicht laden:', e);
        }
        umlagenReady = true;
    }

    // ── Offene Betriebskostenrechnungen ─────────────────────────────────────
    const bkForderungHeaders = [
        { key: 'bezeichnung', value: 'Betriebskostenrechnung' },
        { key: 'verbleibend', value: 'Offen (€)' },
        { key: 'button', value: '' }
    ];

    $: bkForderungTableRows = offeneBkForderungen.map((f) => ({
        id: `bk-${f.buchungssatzId}`,
        bezeichnung: f.bezeichnung,
        verbleibend: f.verbleibend.toFixed(2),
        button: zahlungsButton(() =>
            zahlungErfassen(
                { art: 'bkRechnung', buchungssatzId: f.buchungssatzId },
                f.bezeichnung,
                f.verbleibend
            )
        )
    }));

    $: rechnungsMatrix = baueRechnungsMatrix(
        umlagen,
        selectedYear,
        benoetigteWohnungen
    );
    $: rechnungenOffeneUmlagen = zaehleOffeneBezuege(rechnungsMatrix);

    // ── Garagen (Matrix) ────────────────────────────────────────────────────
    $: garageMatrix = baueGarageMatrix(garageMatrixDaten);
    $: garagenMitOffenerMiete = garageMatrixDaten.filter((z) =>
        z.monate.some((m) => m.offen > 0.005)
    ).length;

    async function garageOeffnen(zelle: MatrixZelle, zeileId: string) {
        const zeile = garageMatrixDaten.find(
            (z) => `${z.garageVertragId}` === zeileId
        );
        const monat = zeile?.monate.find((m) => m.monat === zelle.ref);
        if (!zeile || !monat) return;

        const monatText = new Date(
            monat.monat + 'T00:00:00'
        ).toLocaleDateString('de-DE', { month: 'long', year: 'numeric' });
        zahlungErfassen(
            {
                art: 'garage',
                garageVertragId: zeile.garageVertragId,
                monat: monat.monat,
                kennung: zeile.garage
            },
            `${zeile.bezeichnung} – ${monatText}`,
            Math.max(monat.offen, 0),
            zeile.zahlungsempfaengerId
        );
        mitDetail(
            monat.soll <= 0.005
                ? 'Für diesen Monat ist keine Miete fällig.'
                : monat.offen <= 0.005
                  ? 'Für diesen Monat ist nichts offen.'
                  : 'Für diesen Monat ist noch keine Zahlung eingetragen.',
            monat.offen <= 0.005
        );

        let konto: MietkontoDaten | undefined;
        try {
            konto = (await walter_get(
                `/api/garage-vertraege/${zeile.garageVertragId}/mietkonto`,
                fetchImpl
            )) as MietkontoDaten;
        } catch (e) {
            console.error('Konnte das Garagenmietkonto nicht laden:', e);
        }
        zahlungDetail = mietDetail(
            monat,
            konto,
            `/garage-vertraege/${zeile.garageVertragId}`
        );
        zahlungDetailLaden = false;
    }

    // Alle erfassten Rechnungen werden beim ersten Klick geladen und danach
    // wiederverwendet; nach dem Erfassen einer Rechnung werden sie verworfen.
    let alleRechnungen: RechnungDaten[] | undefined;

    async function ladeRechnungen(): Promise<RechnungDaten[]> {
        if (alleRechnungen) return alleRechnungen;
        try {
            const resp = (await walter_get(
                '/api/betriebskostenrechnungen?take=10000',
                fetchImpl
            )) as { items?: RechnungDaten[] };
            alleRechnungen = resp.items ?? [];
        } catch (e) {
            console.error('Konnte die Rechnungen nicht laden:', e);
            return [];
        }
        return alleRechnungen;
    }

    async function rechnungOeffnen(
        zelle: MatrixZelle,
        zeileId: string,
        spaltenId: string
    ) {
        const umlageId = zelle.ref ?? '';
        rechnungErfassen(zelle, zeileId, spaltenId);
        mitDetail(
            'Für dieses Jahr ist noch keine Rechnung eingetragen.',
            zelle.status === 'erledigt',
            'Weitere Rechnung erfassen'
        );

        const rechnungen = await ladeRechnungen();
        zahlungDetail = rechnungsDetail(
            rechnungen,
            umlageId,
            selectedYear,
            `/umlagen/${umlageId}`
        );
        zahlungDetailLaden = false;
    }

    function rechnungErfassen(
        zelle: MatrixZelle,
        _zeileId: string,
        spaltenId: string
    ) {
        const spalte = rechnungsMatrix.spalten.find((x) => x.id === spaltenId);
        zahlungErfassen(
            { art: 'bkNeu', umlageId: +(zelle.ref ?? 0), jahr: selectedYear },
            `${spalte?.label || 'Betriebskostenrechnung'} ${selectedYear}`,
            0
        );
    }
</script>

<WalterDataWrapperQuickAdd
    title={addZaehlerstandEntry.zaehler?.text || 'Zählerstand'}
    bind:addEntry={addZaehlerstandEntry}
    addUrl={WalterZaehlerstandEntry.ApiURL}
    bind:addModalOpen={addZaehlerstandOpen}
    onSubmit={() => onSubmitZaehlerstand()}
>
    <WalterMatrixDetailInhalt detail={zaehlerDetailDaten} />
    <h5 style="margin: 1.5rem 0 0.5rem">Neu erfassen</h5>
    <WalterZaehlerstand {fetchImpl} entry={addZaehlerstandEntry} />
</WalterDataWrapperQuickAdd>

{#if zahlungZiel}
    <WalterZahlungErfassen
        {fetchImpl}
        ziel={zahlungZiel}
        titel={zahlungTitel}
        vorschlag={zahlungVorschlag}
        vermieterBankkontoId={zahlungEmpfaenger}
        bind:open={zahlungOffen}
        detail={zahlungDetail}
        detailLaden={zahlungDetailLaden}
        detailLeerText={zahlungDetailLeerText}
        formularZu={zahlungFormularZu}
        weitereAktion={zahlungWeitereAktion}
        on:gebucht={nachZahlung}
    />
{/if}

<Tile light>
    <div class="year-selector">
        <NumberInput
            label="Jahr"
            bind:value={selectedYear}
            min={1970}
            max={2099}
            hideSteppers={false}
        />
    </div>

    <div class="tabs">
        <Tabs bind:selected={tab}>
            <Tab
                >Mieten <span
                    class="zahl"
                    class:fertig={vertraegeMitOffenerMiete === 0}
                    >{vertraegeMitOffenerMiete === 0
                        ? '✓'
                        : vertraegeMitOffenerMiete}</span
                ></Tab
            >
            <Tab
                >Garagen <span
                    class="zahl"
                    class:fertig={garagenMitOffenerMiete === 0}
                    >{garagenMitOffenerMiete === 0
                        ? '✓'
                        : garagenMitOffenerMiete}</span
                ></Tab
            >
            <Tab
                >Rechnungen <span
                    class="zahl"
                    class:fertig={rechnungenOffeneUmlagen === 0}
                    >{rechnungenOffeneUmlagen === 0
                        ? '✓'
                        : rechnungenOffeneUmlagen}</span
                ></Tab
            >
            <Tab
                >Zähler <span class="zahl" class:fertig={zaehlerOffen === 0}
                    >{zaehlerOffen === 0 ? '✓' : zaehlerOffen}</span
                ></Tab
            >
            <Tab
                >Abrechnungen <span
                    class="zahl"
                    class:fertig={abrechnungenOffen === 0}
                    >{abrechnungenOffen === 0 ? '✓' : abrechnungenOffen}</span
                ></Tab
            >

            <svelte:fragment slot="content">
                <WalterMatrixWerkzeuge bind:suche bind:nurOffene />

                <TabContent>
                    {#if mietMatrixReady}
                        <WalterMatrix
                            matrix={mietMatrix}
                            {suche}
                            {nurOffene}
                            zeilenLabel="Vertrag"
                            onZelle={mieteOeffnen}
                        />
                    {:else}
                        <SkeletonText style="margin: 0; height: 42px;" />
                    {/if}

                    {#if guthabenReady}
                        <WalterDataTable
                            layout="accordion"
                            accordionTitle={`Guthaben – ${plural(guthabenGruppen.length, 'Vertrag', 'Verträge')}, ${euro(summe(guthabenGruppen))}`}
                            initialOpen={false}
                            rows={guthabenTableRows}
                            headers={guthabenHeaders}
                            rowHref={(row) => row.href}
                        />
                    {:else}
                        <SkeletonText style="margin: 0; height: 42px;" />
                        <div style="height: 2px;" />
                    {/if}
                </TabContent>

                <TabContent>
                    {#if garageMatrixReady}
                        <WalterMatrix
                            matrix={garageMatrix}
                            {suche}
                            {nurOffene}
                            zeilenLabel="Garage"
                            onZelle={garageOeffnen}
                        />
                    {:else}
                        <SkeletonText style="margin: 0; height: 42px;" />
                    {/if}
                </TabContent>

                <TabContent>
                    {#if umlagenReady}
                        <WalterMatrix
                            matrix={rechnungsMatrix}
                            {suche}
                            {nurOffene}
                            zeilenLabel="Wohnung"
                            onZelle={rechnungOeffnen}
                        />
                    {:else}
                        <SkeletonText style="margin: 0; height: 42px;" />
                    {/if}

                    {#if bkForderungenReady}
                        <WalterDataTable
                            layout="accordion"
                            accordionTitle={`Offene Betriebskostenrechnungen – ${bkForderungTableRows.length}`}
                            initialOpen={false}
                            rows={bkForderungTableRows}
                            headers={bkForderungHeaders}
                            on_click_row={(e) => {
                                const row = bkForderungTableRows.find(
                                    (r) => r.id === e.detail.id
                                );
                                row?.button.onClick(new MouseEvent('click'));
                            }}
                        />
                    {:else}
                        <SkeletonText style="margin: 0; height: 42px;" />
                        <div style="height: 2px;" />
                    {/if}
                </TabContent>

                <TabContent>
                    {#if zaehlerMatrixReady}
                        <WalterMatrix
                            matrix={zaehlerMatrix}
                            {suche}
                            {nurOffene}
                            zeilenLabel="Zähler"
                            onZelle={zaehlerOeffnen}
                        />
                    {:else}
                        <SkeletonText style="margin: 0; height: 42px;" />
                    {/if}
                </TabContent>

                <TabContent>
                    {#if abrechnungenReady}
                        <WalterMatrix
                            matrix={abrechnungsMatrix}
                            {suche}
                            {nurOffene}
                            zeilenLabel="Vertrag"
                            onZelle={abrechnungOeffnen}
                        />
                    {:else}
                        <SkeletonText style="margin: 0; height: 42px;" />
                    {/if}
                </TabContent>
            </svelte:fragment>
        </Tabs>
    </div>
</Tile>

<style>
    .year-selector {
        max-width: 10rem;
    }

    .zahl {
        display: inline-block;
        min-width: 1.4rem;
        margin-left: 0.4rem;
        padding: 0 0.4rem;
        border-radius: 0.7rem;
        background: var(--cds-layer-accent, #e0e0e0);
        font-size: 0.75rem;
        line-height: 1.4rem;
        text-align: center;
    }

    .zahl.fertig {
        background: var(--cds-support-success, #24a148);
        color: #fff;
    }

    /* Carbon gibt jedem Tab 10 rem und kürzt längere Namen mit „…“; hier steht
       hinter dem Namen noch die Zahl der offenen Felder. */
    .tabs :global(.bx--tabs__nav-item),
    .tabs :global(a.bx--tabs__nav-link) {
        width: auto;
        min-width: 8rem;
        overflow: visible;
        white-space: nowrap;
        text-overflow: clip;
    }
</style>
