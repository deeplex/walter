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
        WalterZaehlerEntry,
        WalterZaehlerstandEntry,
        type WalterUmlageEntry as WalterUmlageEntryType,
        type WalterZaehlerEntry as WalterZaehlerEntryType,
        type TransaktionsInput
    } from '$walter/lib';
    import { emptyTransaktionsInput } from '$walter/lib';
    import { NumberInput, SkeletonText, Tile } from 'carbon-components-svelte';
    import { WalterDataTable, WalterTransaktion } from '$walter/components';
    import { convertDateCanadian } from '$walter/services/utils';
    import { walter_get } from '$walter/services/requests';
    import WalterZaehlerstand from './WalterZaehlerstand.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import { invalidateAll } from '$app/navigation';
    import { onMount } from 'svelte';

    interface OffeneMietForderungEntry {
        vertragId: number;
        vertragBezeichnung: string;
        monat: string;
        sollbetrag: number;
        gezahlt: number;
        offen: number;
        zahlungsempfaengerId?: number;
    }

    export let fetchImpl: typeof fetch;

    let umlagen: WalterUmlageEntryType[] = [];
    let zaehler: WalterZaehlerEntryType[] = [];
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

    onMount(async () => {
        await ladeOffeneMietForderungen(selectedYear);
        try {
            const [umlPaged, zaehlerPaged] = await Promise.all([
                WalterUmlageEntry.GetPaged<WalterUmlageEntryType>(fetchImpl, {
                    take: 10000
                }),
                WalterZaehlerEntry.GetPaged<WalterZaehlerEntryType>(fetchImpl, {
                    take: 10000
                })
            ]);
            umlagen = umlPaged.items;
            zaehler = zaehlerPaged.items;
        } catch (e) {
            console.error('Konnte Umlagen/Zähler nicht laden:', e);
        }
        umlagenReady = true;
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
    }

    // ── Rent payment QuickAdd ───────────────────────────────────────────────
    let addRentOpen = false;
    let rentModalTitle = 'Mietzahlung';
    let rentBuchungsInput: TransaktionsInput = emptyTransaktionsInput();
    let rentBuchungsInputValid = false;

    function openRentQuickAdd(
        e: CustomEvent,
        forderung: OffeneMietForderungEntry
    ) {
        e.stopPropagation();
        rentModalTitle = forderung.vertragBezeichnung;
        rentBuchungsInput = {
            ...emptyTransaktionsInput(),
            betrag: forderung.offen,
            zahlungsempfaengerId: forderung.zahlungsempfaengerId,
            mieten: [
                {
                    vertragId: forderung.vertragId,
                    betreffenderMonat: forderung.monat,
                    kaltmiete: forderung.offen,
                    nkVorauszahlung: 0
                }
            ]
        };
        addRentOpen = true;
    }

    async function onSubmitRent() {
        await invalidateAll();
        await ladeOffeneMietForderungen(selectedYear);
    }

    const forderungHeaders = [
        { key: 'monat', value: 'Monat' },
        { key: 'vertrag', value: 'Vertrag' },
        { key: 'offen', value: 'Offen (€)' },
        { key: 'button', value: '' }
    ];

    $: forderungTableRows = offeneMietForderungen.map((f) => ({
        id: `forderung-${f.vertragId}-${f.monat}`,
        monat: new Date(f.monat + 'T00:00:00').toLocaleDateString('de-DE', {
            month: 'long',
            year: 'numeric'
        }),
        vertrag: f.vertragBezeichnung,
        offen: f.offen.toFixed(2),
        button: (e: CustomEvent) => openRentQuickAdd(e, f)
    }));

    // ── Meter reading tasks ─────────────────────────────────────────────────
    type MeterTask = { zaehler: WalterZaehlerEntryType };

    function hasMeterReadingForYear(z: WalterZaehlerEntryType, year: number) {
        const inList = (z.staende || []).some(
            (s) => new Date(s.datum).getFullYear() === year
        );
        const lastYear = z.lastZaehlerstand
            ? new Date(z.lastZaehlerstand.datum).getFullYear()
            : undefined;
        return inList || lastYear === year;
    }

    function buildMeterTasks(year: number): MeterTask[] {
        return zaehler
            .filter((z) => !!z.permissions?.update)
            .filter((z) => !z.ende || new Date(z.ende).getFullYear() >= year)
            .filter((z) => !hasMeterReadingForYear(z, year))
            .sort((a, b) => `${a.kennnummer}`.localeCompare(`${b.kennnummer}`))
            .map((z) => ({ zaehler: z }));
    }

    let meterTasks: MeterTask[] = [];
    $: {
        zaehler;
        meterTasks = buildMeterTasks(selectedYear);
    }

    const meterHeaders = [
        { key: 'kennnummer', value: 'Kennnummer' },
        { key: 'type', value: 'Typ' },
        { key: 'button', value: '' }
    ];

    $: meterTableRows = meterTasks.map((task) => ({
        id: `meter-${task.zaehler.id}`,
        kennnummer: task.zaehler.kennnummer,
        type: task.zaehler.typ?.text || 'Zähler',
        button: (e: CustomEvent) => {
            e.stopPropagation();
            openMeterQuickAdd(task);
        }
    }));

    let addZaehlerstandOpen = false;
    let addZaehlerstandEntry: Partial<WalterZaehlerstandEntry> = {};

    function openMeterQuickAdd(task: MeterTask) {
        addZaehlerstandEntry = {
            datum:
                selectedYear === currentYear
                    ? convertDateCanadian(new Date())
                    : convertDateCanadian(new Date(selectedYear, 11, 31)),
            stand: task.zaehler.lastZaehlerstand?.stand,
            einheit: task.zaehler.lastZaehlerstand?.einheit,
            zaehler: { id: `${task.zaehler.id}`, text: task.zaehler.kennnummer }
        };
        addZaehlerstandOpen = true;
    }

    function onSubmitZaehlerstand(newValue: unknown) {
        const value = {
            ...(newValue as WalterZaehlerstandEntry),
            zaehler:
                (newValue as WalterZaehlerstandEntry).zaehler ||
                addZaehlerstandEntry.zaehler
        } as WalterZaehlerstandEntry;

        zaehler.forEach((z) => {
            if (z.id === +value.zaehler?.id) {
                z.staende = [...(z.staende || []), value];
                z.lastZaehlerstand = value;
            }
        });
        zaehler = [...zaehler];
        meterTasks = buildMeterTasks(selectedYear);
        addZaehlerstandOpen = false;
        addZaehlerstandEntry = {};
    }

    // ── Missing billing tasks ───────────────────────────────────────────────
    const billingHeaders = [
        { key: 'typ', value: 'Kostenart' },
        { key: 'wohnungen', value: 'Wohnungen' },
        { key: 'button', value: '' }
    ];

    let addBillingOpen = false;
    let billingModalTitle = 'Betriebskostenrechnung';
    let buchungsInput: TransaktionsInput = emptyTransaktionsInput();
    let buchungsInputValid = false;

    function openBillingQuickAdd(
        e: CustomEvent,
        umlage: WalterUmlageEntryType
    ) {
        e.stopPropagation();
        billingModalTitle = umlage.typ?.text || 'Betriebskostenrechnung';
        buchungsInput = {
            ...emptyTransaktionsInput(),
            betriebskostenEingaenge: [
                {
                    betrag: 0,
                    umlageId: umlage.id,
                    betreffendesJahr: selectedYear
                }
            ]
        };
        addBillingOpen = true;
    }

    async function onSubmitBilling() {
        await invalidateAll();
        try {
            const paged =
                await WalterUmlageEntry.GetPaged<WalterUmlageEntryType>(
                    fetchImpl,
                    { take: 10000 }
                );
            umlagen = paged.items;
        } catch (e) {
            console.error(e);
        }
    }

    $: billingTasks = umlagen.filter(
        (u) =>
            (u.selectedWohnungen || []).length > 0 &&
            !(u.betriebskostenrechnungen || []).some(
                (r) => r.betreffendesJahr === selectedYear
            )
    );

    $: billingTableRows = billingTasks.map((u) => ({
        id: `billing-${u.id}`,
        typ: u.typ?.text || '—',
        wohnungen: (u.selectedWohnungen || []).map((w) => w.text).join(', '),
        button: (e: CustomEvent) => openBillingQuickAdd(e, u)
    }));
</script>

<WalterDataWrapperQuickAdd
    title={rentModalTitle}
    addUrl="/api/transaktionen/buchen"
    bind:addEntry={rentBuchungsInput}
    bind:addModalOpen={addRentOpen}
    onSubmit={onSubmitRent}
    submitDisabled={!rentBuchungsInputValid}
>
    <WalterTransaktion
        {fetchImpl}
        bind:buchung={rentBuchungsInput}
        bind:isValid={rentBuchungsInputValid}
    />
</WalterDataWrapperQuickAdd>

<WalterDataWrapperQuickAdd
    title={addZaehlerstandEntry.zaehler?.text || 'Zählerstand'}
    bind:addEntry={addZaehlerstandEntry}
    addUrl={WalterZaehlerstandEntry.ApiURL}
    bind:addModalOpen={addZaehlerstandOpen}
    onSubmit={onSubmitZaehlerstand}
>
    <WalterZaehlerstand {fetchImpl} entry={addZaehlerstandEntry} />
</WalterDataWrapperQuickAdd>

<WalterDataWrapperQuickAdd
    title={billingModalTitle}
    addUrl="/api/transaktionen/buchen"
    bind:addEntry={buchungsInput}
    bind:addModalOpen={addBillingOpen}
    onSubmit={onSubmitBilling}
    submitDisabled={!buchungsInputValid}
>
    <WalterTransaktion
        {fetchImpl}
        bind:buchung={buchungsInput}
        bind:isValid={buchungsInputValid}
    />
</WalterDataWrapperQuickAdd>

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

    {#if !mietForderungenReady || !umlagenReady}
        <SkeletonText style="margin: 0; height: 42px;" />
        <div style="height: 2px;" />
        <SkeletonText style="margin: 0; height: 42px;" />
        <div style="height: 2px;" />
        <SkeletonText style="margin: 0; height: 42px;" />
    {:else if forderungTableRows.length === 0 && meterTableRows.length === 0 && billingTableRows.length === 0}
        <p style="color: var(--cds-text-02); margin: 1rem;">
            Alles erledigt für {selectedYear}
        </p>
    {:else}
        <WalterDataTable
            layout="accordion"
            accordionTitle="Offene Mietforderungen"
            initialOpen={forderungTableRows.length > 0}
            rows={forderungTableRows}
            headers={forderungHeaders}
        />
        <WalterDataTable
            layout="accordion"
            accordionTitle="Zählerstände fällig"
            rows={meterTableRows}
            headers={meterHeaders}
        />
        <WalterDataTable
            layout="accordion"
            accordionTitle="Rechnungen ausstehend"
            rows={billingTableRows}
            headers={billingHeaders}
        />
        <!-- <WalterDataTable
            layout="accordion"
            accordionTitle="Rechnungen unbezahlt"
        /> -->
    {/if}
</Tile>

<style>
    .year-selector {
        max-width: 10rem;
    }
</style>
