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
        WalterDatePicker,
        WalterLinks,
        WalterLinkTile,
        WalterMonthPicker,
        WalterNumberInput
    } from '$walter/components';
    import { Row } from 'carbon-components-svelte';
    import type {
        WalterMietzahlungInput,
        WalterMietzahlungListEntry,
        WalterVertragEntry
    } from '$walter/lib';

    export let entry: Partial<WalterMietzahlungInput> = {};
    export let vertrag: WalterVertragEntry | undefined = undefined;
    export let mietzahlungen: WalterMietzahlungListEntry[] = [];

    // Local input state — only reset when the vertrag+month context changes.
    let kaltmiete = 0;
    let nk = 0;
    let gesamtbetrag = 0;

    // Reactive display values — always current, independent of user edits.
    $: forderungsbetrag =
        getGrundmiete(vertrag, entry.betreffenderMonat) ||
        +(entry.kaltmieteZahlung ?? 0);
    $: schonGezahlt = getSchonGezahlt(mietzahlungen, entry.betreffenderMonat);
    $: verbleibendeVorZahlung = +Math.max(
        0,
        forderungsbetrag - schonGezahlt
    ).toFixed(2);
    $: nachZahlungVerbleibend = +Math.max(
        0,
        verbleibendeVorZahlung - kaltmiete
    ).toFixed(2);

    // Reinit input fields only when the context (vertrag + month) changes.
    // Changes to mietzahlungen or kaltmiete do not retrigger this.
    let _lastKey = '';
    $: reinitIfNewContext(entry.vertrag?.id, entry.betreffenderMonat);

    function reinitIfNewContext(
        vertragId: string | number | undefined,
        monat: string | undefined
    ) {
        const key = `${vertragId}|${monat}`;
        if (key === _lastKey) return;
        _lastKey = key;

        const grundmiete =
            getGrundmiete(vertrag, monat) || +(entry.kaltmieteZahlung ?? 0);
        const schon = getSchonGezahlt(mietzahlungen, monat);
        kaltmiete = +Math.max(0, grundmiete - schon).toFixed(2);
        nk = +(entry.nkZahlung ?? 0).toFixed(2);
        gesamtbetrag = +(kaltmiete + nk).toFixed(2);
        push();
    }

    function getGrundmiete(
        v: WalterVertragEntry | undefined,
        monat: string | undefined
    ): number {
        if (!v || !monat) return 0;
        const monatDate = new Date(monat);
        const version = [...(v.versionen || [])]
            .filter((ver) => new Date(ver.beginn) <= monatDate)
            .sort(
                (a, b) =>
                    new Date(b.beginn).getTime() - new Date(a.beginn).getTime()
            )[0];
        return version?.grundmiete ?? 0;
    }

    function getSchonGezahlt(
        list: WalterMietzahlungListEntry[],
        monat: string | undefined
    ): number {
        if (!monat) return 0;
        const key = monat.slice(0, 7);
        return list
            .filter((m) => m.betreffenderMonat.slice(0, 7) === key)
            .reduce((sum, m) => sum + m.kaltmieteZahlung, 0);
    }

    function push() {
        entry.kaltmieteZahlung = kaltmiete;
        entry.nkZahlung = nk;
    }

    function onGesamtbetragChange(e: CustomEvent<number | null>) {
        gesamtbetrag = +(e.detail ?? 0).toFixed(2);
        kaltmiete = +Math.min(gesamtbetrag, verbleibendeVorZahlung).toFixed(2);
        nk = +(gesamtbetrag - kaltmiete).toFixed(2);
        push();
    }

    function onKaltmieteChange(e: CustomEvent<number | null>) {
        kaltmiete = +(e.detail ?? 0).toFixed(2);
        nk = +(gesamtbetrag - kaltmiete).toFixed(2);
        push();
    }

    function onNkChange(e: CustomEvent<number | null>) {
        nk = +(e.detail ?? 0).toFixed(2);
        kaltmiete = +(gesamtbetrag - nk).toFixed(2);
        push();
    }
</script>

<Row>
    <WalterMonthPicker
        required
        bind:value={entry.betreffenderMonat}
        labelText="Betreffender Monat"
    />
    <WalterDatePicker
        required
        bind:value={entry.zahlungsdatum}
        labelText="Zahlungsdatum"
    />
</Row>
<Row>
    <WalterNumberInput
        required
        label="Gesamtbetrag"
        value={gesamtbetrag}
        change={onGesamtbetragChange}
    />
</Row>
<Row>
    <WalterNumberInput
        label="Kaltmiete"
        value={kaltmiete}
        change={onKaltmieteChange}
    />
    <WalterNumberInput
        label="Nebenkostenvorauszahlung"
        value={nk}
        change={onNkChange}
    />
</Row>
{#if forderungsbetrag > 0}
    <Row>
        <WalterNumberInput
            readonly
            label="Forderung Kaltmiete"
            value={forderungsbetrag}
        />
        {#if schonGezahlt > 0}
            <WalterNumberInput
                readonly
                label="Davon bereits gezahlt"
                value={schonGezahlt}
            />
            <WalterNumberInput
                readonly
                label="Noch offen"
                value={verbleibendeVorZahlung}
            />
        {/if}
        <WalterNumberInput
            readonly
            label="Nach Zahlung noch offen"
            value={nachZahlungVerbleibend}
        />
    </Row>
{/if}

{#if entry.vertrag?.id}
    <WalterLinks>
        <WalterLinkTile
            fileref=""
            name={`Vertrag: ${entry.vertrag.text || entry.vertrag.id}`}
            href={`/vertraege/${entry.vertrag.id}`}
        />
    </WalterLinks>
{/if}
