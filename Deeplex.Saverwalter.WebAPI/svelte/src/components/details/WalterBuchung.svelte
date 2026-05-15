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
        Button
    } from 'carbon-components-svelte';
    import { Add, TrashCan } from 'carbon-icons-svelte';
    import {
        WalterComboBoxKontakt,
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import WalterBuchungPositionMiete from './WalterBuchungPositionMiete.svelte';
    import WalterBuchungPositionBK from './WalterBuchungPositionBK.svelte';
    import WalterBuchungPositionEA from './WalterBuchungPositionEA.svelte';
    import type {
        TransaktionsInput,
        MietzahlungsInput,
        BetriebskostenEingangInput,
        ErhaltungsaufwendungsInput,
        WalterSelectionEntry
    } from '$walter/lib';

    export let fetchImpl: typeof fetch;
    export let buchung: TransaktionsInput;

    let zahler: WalterSelectionEntry | undefined = undefined;
    let zahlungsempfaenger: WalterSelectionEntry | undefined = undefined;

    $: buchung.zahlerId = zahler?.id as number | undefined;
    $: buchung.zahlungsempfaengerId = zahlungsempfaenger?.id as number | undefined;

    $: totalBetrag =
        buchung.mieten.reduce((s, m) => s + (m.kaltmiete || 0) + (m.nkVorauszahlung || 0), 0) +
        buchung.betriebskostenEingaenge.reduce((s, b) => s + (b.betrag || 0), 0) +
        buchung.erhaltungsaufwendungen.reduce((s, e) => s + (e.betrag || 0), 0) +
        buchung.sonstige.reduce((s, s2) => s + (s2.betrag || 0), 0);

    function addMiete() {
        buchung.mieten = [...buchung.mieten, { kaltmiete: 0, nkVorauszahlung: 0 }];
    }

    function addBK() {
        const currentYear = new Date().getFullYear();
        buchung.betriebskostenEingaenge = [
            ...buchung.betriebskostenEingaenge,
            { betrag: 0, betreffendesJahr: currentYear - 1 }
        ];
    }

    function addEA() {
        buchung.erhaltungsaufwendungen = [
            ...buchung.erhaltungsaufwendungen,
            { betrag: 0 }
        ];
    }

    function removeMiete(i: number) {
        buchung.mieten = buchung.mieten.filter((_, idx) => idx !== i);
    }

    function removeBK(i: number) {
        buchung.betriebskostenEingaenge = buchung.betriebskostenEingaenge.filter((_, idx) => idx !== i);
    }

    function removeEA(i: number) {
        buchung.erhaltungsaufwendungen = buchung.erhaltungsaufwendungen.filter((_, idx) => idx !== i);
    }
</script>

<!-- Transaktion-Felder -->
<Row>
    <Column>
        <WalterComboBoxKontakt {fetchImpl} bind:value={zahler} title="Zahler (optional)" />
    </Column>
    <Column>
        <WalterComboBoxKontakt {fetchImpl} bind:value={zahlungsempfaenger} title="Zahlungsempfänger (optional)" />
    </Column>
</Row>

<Row>
    <Column>
        <WalterNumberInput
            required
            readonly
            label="Gesamtbetrag (€)"
            value={totalBetrag}
        />
    </Column>
    <Column>
        <WalterDatePicker
            required
            labelText="Zahlungsdatum"
            bind:value={buchung.zahlungsdatum}
        />
    </Column>
</Row>

<Row>
    <Column>
        <WalterTextArea
            labelText="Verwendungszweck"
            bind:value={buchung.verwendungszweck}
        />
    </Column>
</Row>

<Row>
    <Column>
        <WalterTextArea labelText="Notiz" bind:value={buchung.notiz} />
    </Column>
</Row>

<hr style="margin: 1rem 0" />

<!-- Mieten -->
{#each buchung.mieten as miete, i (i)}
    <Tile style="margin-bottom: 1rem">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem">
            <strong>Mietzahlung</strong>
            <Button kind="ghost" size="small" icon={TrashCan} iconDescription="Entfernen" on:click={() => removeMiete(i)} />
        </div>
        <WalterBuchungPositionMiete {fetchImpl} bind:miete={buchung.mieten[i]} />
    </Tile>
{/each}

<!-- Betriebskosten -->
{#each buchung.betriebskostenEingaenge as bk, i (i)}
    <Tile style="margin-bottom: 1rem">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem">
            <strong>Betriebskostenrechnung</strong>
            <Button kind="ghost" size="small" icon={TrashCan} iconDescription="Entfernen" on:click={() => removeBK(i)} />
        </div>
        <WalterBuchungPositionBK {fetchImpl} bind:bk={buchung.betriebskostenEingaenge[i]} />
    </Tile>
{/each}

<!-- Erhaltungsaufwendungen -->
{#each buchung.erhaltungsaufwendungen as ea, i (i)}
    <Tile style="margin-bottom: 1rem">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 0.5rem">
            <strong>Erhaltungsaufwendung</strong>
            <Button kind="ghost" size="small" icon={TrashCan} iconDescription="Entfernen" on:click={() => removeEA(i)} />
        </div>
        <WalterBuchungPositionEA {fetchImpl} bind:ea={buchung.erhaltungsaufwendungen[i]} />
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
