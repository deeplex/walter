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
        WalterUmlageEntry,
        WalterZaehlerstandEntry,
        type WalterUmlageEntry as WalterUmlageEntryType,
        type WalterZahlungsZiel
    } from '$walter/lib';
    import { NumberInput, SkeletonText, Tile } from 'carbon-components-svelte';
    import { WalterDataTable } from '$walter/components';
    import WalterZahlungErfassen from './WalterZahlungErfassen.svelte';
    import { convertDateCanadian } from '$walter/services/utils';
    import { walter_get } from '$walter/services/requests';
    import WalterZaehlerstand from './WalterZaehlerstand.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import { onMount } from 'svelte';

    interface OffeneBkForderungEntry {
        buchungssatzId: string;
        bezeichnung: string;
        betrag: number;
        schonGezahlt: number;
        verbleibend: number;
    }

    interface OffeneGarageInfo {
        garageVertragId: number;
        garageKennung: string;
        offen: number;
    }

    interface OffeneMietForderungEntry {
        vertragId: number;
        vertragBezeichnung: string;
        monat: string;
        kaltmieteOffen: number;
        offen: number;
        zahlungsempfaengerId?: number;
        garagen: OffeneGarageInfo[];
    }

    interface OffeneGarageForderungEntry {
        garageVertragId: number;
        bezeichnung: string;
        monat: string;
        offen: number;
        zahlungsempfaengerId?: number;
    }

    interface GuthabenEntry {
        vertragId: number;
        vertragBezeichnung: string;
        monat: string;
        guthaben: number;
    }

    interface ZaehlerstandFaelligEntry {
        id: number;
        kennnummer: string;
        typText: string;
        lastStand?: number;
        lastEinheit?: string;
    }

    export let fetchImpl: typeof fetch;

    let umlagen: WalterUmlageEntryType[] = [];
    let zaehlerFaellig: ZaehlerstandFaelligEntry[] = [];
    let umlagenReady = false;

    // ── Offene Mietforderungen ──────────────────────────────────────────────
    let offeneMietForderungen: OffeneMietForderungEntry[] = [];
    let mietForderungenReady = false;

    async function ladeOffeneMietForderungen(jahr: number) {
        mietForderungenReady = false;
        try {
            const resp = await walter_get(
                `/api/offene-forderungen/${jahr}`,
                fetchImpl
            );
            offeneMietForderungen = (resp as OffeneMietForderungEntry[]) ?? [];
        } catch (e) {
            console.error('Konnte offene Forderungen nicht laden:', e);
            offeneMietForderungen = [];
        }
        mietForderungenReady = true;
    }

    // ── Offene Garagenforderungen (standalone) ──────────────────────────────
    let offeneGarageForderungen: OffeneGarageForderungEntry[] = [];
    let garageForderungenReady = false;

    async function ladeOffeneGarageForderungen(jahr: number) {
        garageForderungenReady = false;
        try {
            const resp = await walter_get(
                `/api/offene-forderungen/garagen/${jahr}`,
                fetchImpl
            );
            offeneGarageForderungen =
                (resp as OffeneGarageForderungEntry[]) ?? [];
        } catch (e) {
            console.error('Konnte offene Garagenforderungen nicht laden:', e);
            offeneGarageForderungen = [];
        }
        garageForderungenReady = true;
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

    async function ladeZaehlerFaellig(jahr: number) {
        try {
            const resp = await walter_get(
                `/api/offene-forderungen/zaehlerstaende-faellig/${jahr}`,
                fetchImpl
            );
            zaehlerFaellig = (resp as ZaehlerstandFaelligEntry[]) ?? [];
        } catch (e) {
            console.error('Konnte fällige Zählerstände nicht laden:', e);
            zaehlerFaellig = [];
        }
    }

    onMount(async () => {
        await Promise.all([
            ladeOffeneMietForderungen(selectedYear),
            ladeOffeneGarageForderungen(selectedYear),
            ladeOffeneBkForderungen(selectedYear),
            ladeGuthaben(selectedYear),
            ladeZaehlerFaellig(selectedYear)
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

    let _lastLoadedYear = selectedYear;
    $: if (selectedYear !== _lastLoadedYear) {
        _lastLoadedYear = selectedYear;
        ladeOffeneMietForderungen(selectedYear);
        ladeOffeneGarageForderungen(selectedYear);
        ladeOffeneBkForderungen(selectedYear);
        ladeGuthaben(selectedYear);
        ladeZaehlerFaellig(selectedYear);
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
                    ladeOffeneMietForderungen(selectedYear),
                    ladeGuthaben(selectedYear)
                ]);
                break;
            case 'garage':
                await ladeOffeneGarageForderungen(selectedYear);
                break;
            case 'bkRechnung':
                await ladeOffeneBkForderungen(selectedYear);
                break;
            case 'bkNeu':
                await ladeUmlagen();
                break;
        }
    }

    const monatText = (monat: string) =>
        new Date(monat + 'T00:00:00').toLocaleDateString('de-DE', {
            month: 'long',
            year: 'numeric'
        });

    const zahlungsButton = (onClick: () => void) => ({
        onClick: (e: MouseEvent) => {
            e.stopPropagation();
            onClick();
        },
        iconDescription: 'Zahlung erfassen'
    });

    const forderungHeaders = [
        { key: 'monat', value: 'Monat' },
        { key: 'vertrag', value: 'Vertrag' },
        { key: 'offen', value: 'Offen (€)' },
        { key: 'button', value: '' }
    ];

    const garageForderungHeaders = [
        { key: 'monat', value: 'Monat' },
        { key: 'bezeichnung', value: 'Garage' },
        { key: 'offen', value: 'Offen (€)' },
        { key: 'button', value: '' }
    ];

    $: garageForderungTableRows = offeneGarageForderungen.map((f) => ({
        id: `garage-${f.garageVertragId}-${f.monat}`,
        href: `/garage-vertraege/${f.garageVertragId}`,
        monat: monatText(f.monat),
        bezeichnung: f.bezeichnung,
        offen: f.offen.toFixed(2),
        button: zahlungsButton(() =>
            zahlungErfassen(
                {
                    art: 'garage',
                    garageVertragId: f.garageVertragId,
                    monat: f.monat
                },
                `${f.bezeichnung} – ${monatText(f.monat)}`,
                f.offen,
                f.zahlungsempfaengerId
            )
        )
    }));

    $: forderungTableRows = offeneMietForderungen.map((f) => ({
        id: `forderung-${f.vertragId}-${f.monat}`,
        href: `/vertraege/${f.vertragId}#mietkonto`,
        monat: monatText(f.monat),
        vertrag: f.vertragBezeichnung,
        offen: f.offen.toFixed(2),
        button: zahlungsButton(() =>
            zahlungErfassen(
                { art: 'miete', vertragId: f.vertragId, monat: f.monat },
                `${f.vertragBezeichnung} – ${monatText(f.monat)}`,
                f.offen,
                f.zahlungsempfaengerId
            )
        )
    }));

    // ── Meter reading tasks ─────────────────────────────────────────────────
    const meterHeaders = [
        { key: 'kennnummer', value: 'Kennnummer' },
        { key: 'type', value: 'Typ' }
    ];

    $: meterTableRows = zaehlerFaellig.map((z) => ({
        id: `meter-${z.id}`,
        kennnummer: z.kennnummer,
        type: z.typText || 'Zähler',
        button: () => openMeterQuickAdd(z)
    }));

    let addZaehlerstandOpen = false;
    let addZaehlerstandEntry: Partial<WalterZaehlerstandEntry> = {};

    function openMeterQuickAdd(z: ZaehlerstandFaelligEntry) {
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
        await ladeZaehlerFaellig(selectedYear);
    }

    // ── Missing billing tasks ───────────────────────────────────────────────
    const billingHeaders = [
        { key: 'typ', value: 'Kostenart' },
        { key: 'wohnungen', value: 'Wohnungen' },
        { key: 'button', value: '' }
    ];

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

    $: billingTasks = umlagen.filter(
        (u) =>
            (u.selectedWohnungen || []).length > 0 &&
            (!u.ende || new Date(u.ende).getFullYear() >= selectedYear) &&
            !(u.betriebskostenrechnungsJahre || []).includes(selectedYear)
    );

    $: billingTableRows = billingTasks.map((u) => ({
        id: `billing-${u.id}`,
        typ: u.typ?.text || '—',
        wohnungen: (u.selectedWohnungen || []).map((w) => w.text).join(', '),
        button: zahlungsButton(() =>
            zahlungErfassen(
                { art: 'bkNeu', umlageId: u.id, jahr: selectedYear },
                `${u.typ?.text || 'Betriebskostenrechnung'} ${selectedYear}`,
                0
            )
        )
    }));

    // ── Guthaben ────────────────────────────────────────────────────────────
    const guthabenHeaders = [
        { key: 'monat', value: 'Monat' },
        { key: 'vertrag', value: 'Vertrag' },
        { key: 'guthaben', value: 'Guthaben (€)' }
    ];

    $: guthabenTableRows = guthaben.map((g) => ({
        id: `guthaben-${g.vertragId}-${g.monat}`,
        href: `/vertraege/${g.vertragId}#mietkonto`,
        monat: monatText(g.monat),
        vertrag: g.vertragBezeichnung,
        guthaben: g.guthaben.toFixed(2)
    }));
</script>

<WalterDataWrapperQuickAdd
    title={addZaehlerstandEntry.zaehler?.text || 'Zählerstand'}
    bind:addEntry={addZaehlerstandEntry}
    addUrl={WalterZaehlerstandEntry.ApiURL}
    bind:addModalOpen={addZaehlerstandOpen}
    onSubmit={() => onSubmitZaehlerstand()}
>
    <WalterZaehlerstand {fetchImpl} entry={addZaehlerstandEntry} />
</WalterDataWrapperQuickAdd>

{#if zahlungZiel}
    <WalterZahlungErfassen
        {fetchImpl}
        ziel={zahlungZiel}
        titel={zahlungTitel}
        vorschlag={zahlungVorschlag}
        zahlungsempfaengerId={zahlungEmpfaenger}
        bind:open={zahlungOffen}
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

    {#if mietForderungenReady}
        <WalterDataTable
            layout="accordion"
            accordionTitle="Offene Mieten"
            initialOpen={forderungTableRows.length > 0}
            rows={forderungTableRows}
            headers={forderungHeaders}
            rowHref={(row) => row.href}
        />
    {:else}
        <SkeletonText style="margin: 0; height: 42px;" />
        <div style="height: 2px;" />
    {/if}

    {#if garageForderungenReady}
        <WalterDataTable
            layout="accordion"
            accordionTitle="Offene Garagenmieten"
            initialOpen={garageForderungTableRows.length > 0}
            rows={garageForderungTableRows}
            headers={garageForderungHeaders}
            rowHref={(row) => row.href}
        />
    {:else}
        <SkeletonText style="margin: 0; height: 42px;" />
        <div style="height: 2px;" />
    {/if}

    {#if umlagenReady}
        <WalterDataTable
            layout="accordion"
            accordionTitle="Fehlende Betriebskostenrechnungen"
            initialOpen={billingTableRows.length > 0}
            rows={billingTableRows}
            headers={billingHeaders}
            on_click_row={(e) => {
                const row = billingTableRows.find((r) => r.id === e.detail.id);
                row?.button.onClick(new MouseEvent('click'));
            }}
        />
    {:else}
        <SkeletonText style="margin: 0; height: 42px;" />
        <div style="height: 2px;" />
    {/if}

    {#if bkForderungenReady}
        <WalterDataTable
            layout="accordion"
            accordionTitle="Offene Betriebskostenrechnungen"
            initialOpen={bkForderungTableRows.length > 0}
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

    {#if guthabenReady}
        <WalterDataTable
            layout="accordion"
            accordionTitle="Guthaben"
            initialOpen={guthabenTableRows.length > 0}
            rows={guthabenTableRows}
            headers={guthabenHeaders}
            rowHref={(row) => row.href}
        />
    {:else}
        <SkeletonText style="margin: 0; height: 42px;" />
        <div style="height: 2px;" />
    {/if}

    <WalterDataTable
        layout="accordion"
        accordionTitle="Zählerstände fällig"
        rows={meterTableRows}
        headers={meterHeaders}
        on_click_row={(e) => {
            const row = meterTableRows.find((r) => r.id === e.detail.id);
            if (row?.button) row.button();
        }}
    />
</Tile>

<style>
    .year-selector {
        max-width: 10rem;
    }
</style>
