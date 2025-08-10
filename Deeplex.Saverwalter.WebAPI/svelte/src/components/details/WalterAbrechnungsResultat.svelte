<!-- Copyright (C) 2023-2024  Kai Lawrence -->
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
    import { WalterLinks, WalterNumberInput } from '$walter/components';
    import {
        Checkbox,
        ClickableTile,
        Row,
        Tile
    } from 'carbon-components-svelte';
    import WalterLinkTile from '../subdetails/WalterLinkTile.svelte';
    import { fileURL } from '$walter/services/files';
    import { WalterAbrechnungsresultatEntry } from '$walter/lib/WalterAbrechnungsresultat';
    import WalterTextArea from '../elements/WalterTextArea.svelte';
    import { convertEuro } from '$walter/services/utils';

    export let entry: WalterAbrechnungsresultatEntry;

    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }

    const abgesendet = (e: Event) => {
        entry.abgesendet = (e.target as HTMLInputElement).checked;
    };

    let editResult = false;
</script>

<Row>
    <WalterNumberInput required readonly value={entry.jahr} label="Jahr" />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.saldo}
        label="Verbleibender Saldo (Positiv = Mieter muss zahlen, Negativ = Vermieter muss zahlen)"
    />

    <Tile light style="margin: 1.5em">
        <Checkbox
            labelText="Ist diese Abrechnung an den Mieter versendet?"
            bind:checked={entry.abgesendet}
            on:change={(e) => abgesendet(e)}
        />
    </Tile>
</Row>

<Row>
    <div>
        <Checkbox bind:checked={editResult} on:change={(e) => abgesendet(e)} />
    </div>
    <p style="margin-top: 0.25em; margin-left: -0.5em">
        Ergebnisse der Abrechnung bearbeiten
    </p>
</Row>

<Row>
    <WalterNumberInput
        required
        readonly={!editResult}
        bind:value={entry.vorauszahlung}
        label="Vorauszahlung Gesamt"
    />

    <WalterNumberInput
        required
        readonly={!editResult}
        bind:value={entry.kaltmiete}
        label="Kaltmiete"
    />

    <WalterNumberInput
        required
        readonly={!editResult}
        bind:value={entry.rechnungsbetrag}
        label="Rechnungsbetrag"
    />

    <WalterNumberInput
        required
        readonly={!editResult}
        bind:value={entry.minderung}
        label="Mietminderungen"
    />
</Row>

<Row>
    <Tile light>
        Resultat der Abrechnung: {convertEuro(
            entry.vorauszahlung - entry.kaltmiete - entry.rechnungsbetrag
        )}
        {entry.vorauszahlung - entry.kaltmiete - entry.rechnungsbetrag < 0
            ? ' (Schulden des Mieters)'
            : ' (Guthaben des Mieters)'}
    </Tile>
</Row>

<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>

<WalterLinks>
    <WalterLinkTile
        fileref={fileURL.vertrag(`${entry.vertrag?.id}`)}
        name={`Vertrag: ${entry?.vertrag?.text || 'ansehen'}`}
        href={`/vertraege/${entry.vertrag?.id}`}
    />

    <ClickableTile
        href={`/abrechnung/?jahr=${entry.jahr}&vertrag=${entry.vertrag?.id}`}
    >
        Abrechnung ansehen
    </ClickableTile>
</WalterLinks>
