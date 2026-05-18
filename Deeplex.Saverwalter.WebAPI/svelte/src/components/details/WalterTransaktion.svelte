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
    import {
        emptyTransaktionsInput,
        type TransaktionsInput,
        type WalterSelectionEntry,
        type WalterTransaktionEntry
    } from '$walter/lib';
    import WalterTransaktionRaw from './WalterTransaktionRaw.svelte';

    export let fetchImpl: typeof fetch;
    export let buchung: TransaktionsInput = emptyTransaktionsInput();
    let transaktionEntry: Partial<WalterTransaktionEntry> = {};
    export let isValid = false;

    let zahler: WalterSelectionEntry | undefined = undefined;
    let zahlungsempfaenger: WalterSelectionEntry | undefined = undefined;

    $: buchung.zahlerId = (zahler as WalterSelectionEntry | undefined)?.id as
        | number
        | undefined;
    $: buchung.zahlungsempfaengerId = (
        zahlungsempfaenger as WalterSelectionEntry | undefined
    )?.id as number | undefined;

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
    $: isValid = buchung.betrag > 0 && Math.abs(offenerBetrag) < 0.005;

    function addMiete() {
        const available = Math.max(0, offenerBetrag);
        buchung.mieten = [
            ...buchung.mieten,
            { kaltmiete: available, nkVorauszahlung: 0 }
        ];
    }

    function addBK() {
        const currentYear = new Date().getFullYear();
        const available = Math.max(0, offenerBetrag);
        buchung.betriebskostenEingaenge = [
            ...buchung.betriebskostenEingaenge,
            { betrag: available, betreffendesJahr: currentYear - 1 }
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
</script>

<WalterTransaktionRaw bind:entry={transaktionEntry} {fetchImpl} readonly />

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
            bind:availableBetrag={transaktionEntry.betrag}
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
        </div>
    </Column>
</Row>
