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
        type WalterZaehlerEntry,
        type TransaktionsInput
    } from '$walter/lib';
    import { emptyTransaktionsInput } from '$walter/lib';
    import { NumberInput } from 'carbon-components-svelte';
    import { WalterDataTable } from '$walter/components';
    import { convertDateCanadian } from '$walter/services/utils';
    import WalterZaehlerstand from './WalterZaehlerstand.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import WalterBuchung from './WalterBuchung.svelte';
    import { invalidateAll } from '$app/navigation';
    import { onMount } from 'svelte';

    export let fetchImpl: typeof fetch;

    let umlagen: WalterUmlageEntryType[] = [];
    let umlagenReady = false;

    onMount(async () => {
        try {
            umlagen =
                await WalterUmlageEntry.GetAll<WalterUmlageEntryType>(
                    fetchImpl
                );
        } catch (e) {
            console.error('Konnte Umlagen nicht laden:', e);
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

    // ── Meter reading tasks ─────────────────────────────────────────────────
    type MeterTask = { zaehler: WalterZaehlerEntry };

    function hasMeterReadingForYear(zaehler: WalterZaehlerEntry, year: number) {
        const inList = (zaehler.staende || []).some(
            (s) => new Date(s.datum).getFullYear() === year
        );
        const lastYear = zaehler.lastZaehlerstand
            ? new Date(zaehler.lastZaehlerstand.datum).getFullYear()
            : undefined;
        return inList || lastYear === year;
    }

    function buildMeterTasks(year: number): MeterTask[] {
        const byId = new Map<number, WalterZaehlerEntry>();
        umlagen.forEach((u) =>
            (u.zaehler || []).forEach((z) => {
                if (!byId.has(z.id)) byId.set(z.id, z);
            })
        );
        return [...byId.values()]
            .filter((z) => !!z.permissions?.update)
            .filter((z) => !z.ende || new Date(z.ende).getFullYear() >= year)
            .filter((z) => !hasMeterReadingForYear(z, year))
            .sort((a, b) => `${a.kennnummer}`.localeCompare(`${b.kennnummer}`))
            .map((z) => ({ zaehler: z }));
    }

    let meterTasks: MeterTask[] = [];
    $: {
        umlagen;
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

        umlagen.forEach((u) =>
            (u.zaehler || []).forEach((z) => {
                if (z.id === +value.zaehler?.id) {
                    z.staende = [...(z.staende || []), value];
                    z.lastZaehlerstand = value;
                }
            })
        );
        umlagen = [...umlagen];
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
            umlagen =
                await WalterUmlageEntry.GetAll<WalterUmlageEntryType>(
                    fetchImpl
                );
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
>
    <WalterBuchung {fetchImpl} bind:buchung={buchungsInput} />
</WalterDataWrapperQuickAdd>

<div class="year-selector">
    <NumberInput
        label="Jahr"
        bind:value={selectedYear}
        min={2000}
        max={2099}
        hideSteppers={false}
    />
</div>

{#if !umlagenReady}
    <p style="color: var(--cds-text-02); margin-top: 1rem;">
        Daten werden geladen…
    </p>
{:else if meterTableRows.length === 0 && billingTableRows.length === 0}
    <p style="color: var(--cds-text-02); margin-top: 1rem;">
        Alles erledigt für {selectedYear} ✓
    </p>
{:else}
    <div style="margin-top: 1rem;">
        <WalterDataTable
            layout="accordion"
            accordionTitle="Zählerstände fällig"
            rows={meterTableRows}
            headers={meterHeaders}
        />
        <WalterDataTable
            layout="accordion"
            accordionTitle="Abrechnungen ausstehend"
            rows={billingTableRows}
            headers={billingHeaders}
        />
    </div>
{/if}

<style>
    .year-selector {
        max-width: 10rem;
    }
</style>
