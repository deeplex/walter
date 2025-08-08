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
        Link,
        Row
    } from 'carbon-components-svelte';
    import WalterLinkTile from '../subdetails/WalterLinkTile.svelte';
    import { fileURL } from '$walter/services/files';
    import { WalterAbrechnungsresultatEntry } from '$walter/lib/WalterAbrechnungsresultat';
    import WalterTextArea from '../elements/WalterTextArea.svelte';

    export let entry: WalterAbrechnungsresultatEntry;

    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false;
    }

    const abgesendet = (e: Event) => {
        entry.abgesendet = (e.target as HTMLInputElement).checked;
    };

    const istBeglichen = (e: Event) => {
        entry.istBeglichen = (e.target as HTMLInputElement).checked;
    };
</script>

<Row>
    <WalterNumberInput required readonly value={entry.jahr} label="Jahr" />

    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.vorauszahlung}
        label="Vorauszahlung Gesamt"
    />

    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.kaltmiete}
        label="Kaltmiete"
    />

    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.rechnungsbetrag}
        label="Rechnungsbetrag"
    />

    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.minderung}
        label="Mietminderungen"
    />
</Row>

<Row>
    <div>
        <Checkbox
            bind:checked={entry.abgesendet}
            on:change={(e) => abgesendet(e)}
        />
    </div>
    <p style="margin-top: 0.25em; margin-left: -0.5em">
        Ist diese Abrechnung an den Mieter versendet?
    </p>
</Row>

<Row>
    <div>
        <Checkbox
            bind:checked={entry.istBeglichen}
            on:change={(e) => istBeglichen(e)}
        />
    </div>
    <p style="margin-top: 0.25em; margin-left: -0.5em">
        Ist diese Abrechnung beglichen?
    </p>
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
