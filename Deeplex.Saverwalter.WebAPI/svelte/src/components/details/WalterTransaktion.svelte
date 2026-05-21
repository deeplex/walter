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
        Row,
        Column,
        Tile,
        Button,
        InlineNotification
    } from 'carbon-components-svelte';
    import { Add, TrashCan } from 'carbon-icons-svelte';
    import WalterTransaktionPositionMiete from './WalterTransaktionPositionMiete.svelte';
    import WalterTransaktionPositionBK from './WalterTransaktionPositionBK.svelte';
    import WalterTransaktionPositionEA from './WalterTransaktionPositionEA.svelte';
    import WalterTransaktionPositionSonstiges from './WalterTransaktionPositionSonstiges.svelte';
    import {
        emptyTransaktionsInput,
        type TransaktionsInput,
        type WalterTransaktionEntry
    } from '$walter/lib';
    import WalterTransaktionRaw from './WalterTransaktionRaw.svelte';

    export let fetchImpl: typeof fetch;
    export let buchung: TransaktionsInput = emptyTransaktionsInput();
    export let isValid = false;

    let transaktionEntry: Partial<WalterTransaktionEntry> = {};

    // Reset display state (zahler/zahlungsempfaenger) when parent swaps buchung object
    let _prevBuchung = buchung;
    $: if (buchung !== _prevBuchung) {
        _prevBuchung = buchung;
        transaktionEntry = {};
    }

    $: verteilterBetrag =
        buchung.mieten.reduce(
            (s, m) => s + (m.kaltmiete || 0) + (m.nkVorauszahlung || 0),
            0
        ) +
        buchung.betriebskostenEingaenge.reduce(
            (s, b) => s + (b.betrag || 0),
            0
        ) +
        buchung.erhaltungsaufwendungen.reduce(
            (s, e) => s + (e.betrag || 0),
            0
        ) +
        buchung.sonstige.reduce((s, s2) => s + (s2.betrag || 0), 0);

    $: totalPositions =
        buchung.mieten.length +
        buchung.betriebskostenEingaenge.length +
        buchung.erhaltungsaufwendungen.length +
        buchung.sonstige.length;

    $: isSinglePosition = totalPositions === 1;

    $: offenerBetrag = (buchung.betrag || 0) - verteilterBetrag;

    let mieteInvalids: boolean[] = [];
    $: isValid =
        buchung.betrag > 0 &&
        Math.abs(offenerBetrag) < 0.005 &&
        !mieteInvalids.some(Boolean);

    function addMiete() {
        const available = Math.max(0, offenerBetrag);
        buchung.mieten = [
            ...buchung.mieten,
            { kaltmiete: available, nkVorauszahlung: 0 }
        ];
    }

    function addBK() {
        const available = Math.max(0, offenerBetrag);
        buchung.betriebskostenEingaenge = [
            ...buchung.betriebskostenEingaenge,
            { betrag: available, betreffendesJahr: new Date().getFullYear() - 1 }
        ];
    }

    function addEA() {
        const available = Math.max(0, offenerBetrag);
        buchung.erhaltungsaufwendungen = [
            ...buchung.erhaltungsaufwendungen,
            { betrag: available }
        ];
    }

    function removeMiete(i: number) {
        buchung.mieten = buchung.mieten.filter((_, idx) => idx !== i);
    }

    function removeBK(i: number) {
        buchung.betriebskostenEingaenge =
            buchung.betriebskostenEingaenge.filter((_, idx) => idx !== i);
    }

    function removeEA(i: number) {
        buchung.erhaltungsaufwendungen = buchung.erhaltungsaufwendungen.filter(
            (_, idx) => idx !== i
        );
    }

    function addSonstiges() {
        const available = Math.max(0, offenerBetrag);
        buchung.sonstige = [...buchung.sonstige, { betrag: available }];
    }

    function removeSonstiges(i: number) {
        buchung.sonstige = buchung.sonstige.filter((_, idx) => idx !== i);
    }

    function onZahlerChange(e: CustomEvent) {
        buchung.zahlerId = e.detail ? +e.detail.id || undefined : undefined;
    }

    function onZahlungsempfaengerChange(e: CustomEvent) {
        buchung.zahlungsempfaengerId = e.detail ? +e.detail.id || undefined : undefined;
    }
</script>

<WalterTransaktionRaw
    bind:entry={transaktionEntry}
    bind:betrag={buchung.betrag}
    bind:zahlungsdatum={buchung.zahlungsdatum}
    {fetchImpl}
    initialZahlungsempfaengerId={buchung.zahlungsempfaengerId}
    on:zahlerChange={onZahlerChange}
    on:zahlungsempfaengerChange={onZahlungsempfaengerChange}
/>

{#if buchung.betrag > 0 && Math.abs(offenerBetrag) >= 0.005}
    <InlineNotification
        kind="error"
        title="Betrag nicht vollständig verteilt:"
        subtitle="Noch {offenerBetrag > 0
            ? offenerBetrag.toFixed(2) + ' € offen'
            : Math.abs(offenerBetrag).toFixed(2) + ' € zu viel verteilt'}"
        hideCloseButton
    />
{/if}

<hr style="margin: 1rem 0" />

<!-- Mieten -->
{#each buchung.mieten as miete, i (i)}
    <Tile style="margin-bottom: 1rem">
        <div
            style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem"
        >
            <strong>Mietzahlung</strong>
            <Button
                kind="ghost"
                size="small"
                icon={TrashCan}
                iconDescription="Entfernen"
                on:click={() => removeMiete(i)}
            />
        </div>
        <WalterTransaktionPositionMiete
            {fetchImpl}
            bind:miete={buchung.mieten[i]}
            bind:availableBetrag={buchung.betrag}
            bind:invalid={mieteInvalids[i]}
            {isSinglePosition}
        />
    </Tile>
{/each}

<!-- Betriebskosten -->
{#each buchung.betriebskostenEingaenge as bk, i (i)}
    <Tile style="margin-bottom: 1rem">
        <div
            style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem"
        >
            <strong>Betriebskostenrechnung</strong>
            <Button
                kind="ghost"
                size="small"
                icon={TrashCan}
                iconDescription="Entfernen"
                on:click={() => removeBK(i)}
            />
        </div>
        <WalterTransaktionPositionBK
            {fetchImpl}
            bind:bk={buchung.betriebskostenEingaenge[i]}
            availableBetrag={buchung.betrag ?? 0}
            {isSinglePosition}
        />
    </Tile>
{/each}

<!-- Erhaltungsaufwendungen -->
{#each buchung.erhaltungsaufwendungen as ea, i (i)}
    <Tile style="margin-bottom: 1rem">
        <div
            style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem"
        >
            <strong>Erhaltungsaufwendung</strong>
            <Button
                kind="ghost"
                size="small"
                icon={TrashCan}
                iconDescription="Entfernen"
                on:click={() => removeEA(i)}
            />
        </div>
        <WalterTransaktionPositionEA
            {fetchImpl}
            bind:ea={buchung.erhaltungsaufwendungen[i]}
            availableBetrag={buchung.betrag ?? 0}
            {isSinglePosition}
        />
    </Tile>
{/each}

<!-- Sonstiges -->
{#each buchung.sonstige as _, i (i)}
    <Tile style="margin-bottom: 1rem">
        <div
            style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem"
        >
            <strong>Sonstiges</strong>
            <Button
                kind="ghost"
                size="small"
                icon={TrashCan}
                iconDescription="Entfernen"
                on:click={() => removeSonstiges(i)}
            />
        </div>
        <WalterTransaktionPositionSonstiges
            bind:sonstiger={buchung.sonstige[i]}
            availableBetrag={buchung.betrag ?? 0}
            {isSinglePosition}
        />
    </Tile>
{/each}

<!-- Position hinzufügen -->
<Row style="margin-bottom: 1.5rem">
    <Column>
        <div style="display: flex; gap: 0.5rem; flex-wrap: wrap">
            <Button kind="tertiary" size="small" icon={Add} on:click={addMiete}>
                Miete
            </Button>
            <Button kind="tertiary" size="small" icon={Add} on:click={addBK}>
                Betriebskostenrechnung
            </Button>
            <Button kind="tertiary" size="small" icon={Add} on:click={addEA}>
                Erhaltungsaufwendung
            </Button>
            <Button kind="tertiary" size="small" icon={Add} on:click={addSonstiges}>
                Sonstiges
            </Button>
        </div>
    </Column>
</Row>
