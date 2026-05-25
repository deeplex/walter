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
        DataTable,
        InlineNotification,
        Row,
        Tile,
        Toolbar,
        ToolbarContent
    } from 'carbon-components-svelte';
    import WalterLinkTile from '../subdetails/WalterLinkTile.svelte';
    import { fileURL } from '$walter/services/files';
    import type { WalterAbrechnungsresultatEntry } from '$walter/lib/WalterAbrechnungsresultat';
    import WalterTextArea from '../elements/WalterTextArea.svelte';
    import { convertEuro } from '$walter/services/utils';

    export let entry: WalterAbrechnungsresultatEntry;

    export let readonly = false;
    $: {
        readonly = entry?.permissions?.update === false || entry?.abgesendet === true;
    }

    const abgesendet = (e: Event) => {
        entry.abgesendet = (e.target as HTMLInputElement).checked;
    };
</script>

{#if entry.abgesendet}
    <Row>
        <InlineNotification
            kind="info"
            title="Abgesendet:"
            subtitle="Diese Abrechnung wurde versendet und ist gesperrt. Zum Ändern bitte zuerst stornieren."
            hideCloseButton
        />
    </Row>
{/if}
<Row>
    <WalterNumberInput required readonly value={entry.jahr} label="Jahr" />
    <WalterNumberInput
        required
        readonly
        value={entry.saldo}
        label="Saldo (Positiv = Mieter muss nachzahlen, Negativ = Vermieter erstattet)"
    />

    <Tile light style="margin: 1.5em">
        <Checkbox
            disabled={readonly}
            labelText="Ist diese Abrechnung an den Mieter versendet?"
            bind:checked={entry.abgesendet}
            on:change={abgesendet}
        />
    </Tile>
</Row>

{#if entry.nkKontoZeilen?.length > 0}
    {@const sollSumme = entry.nkKontoZeilen
        .filter((z) => z.istSoll)
        .reduce((s, z) => s + z.betrag, 0)}
    {@const habenSumme = entry.nkKontoZeilen
        .filter((z) => !z.istSoll)
        .reduce((s, z) => s + z.betrag, 0)}
    <Row>
        <DataTable
            title="Nebenkostenkonto {entry.jahr}"
            size="short"
            headers={[
                { key: 'datum', value: 'Datum' },
                { key: 'beschreibung', value: 'Buchung' },
                { key: 'soll', value: 'Soll' },
                { key: 'haben', value: 'Haben' }
            ]}
            rows={[
                ...entry.nkKontoZeilen.map((z, i) => ({
                    id: String(i),
                    datum: z.datum,
                    beschreibung: z.beschreibung,
                    soll: z.istSoll ? convertEuro(z.betrag) : '',
                    haben: !z.istSoll ? convertEuro(z.betrag) : ''
                })),
                {
                    id: 'summe',
                    datum: '',
                    beschreibung: 'Summe',
                    soll: convertEuro(sollSumme),
                    haben: convertEuro(habenSumme)
                }
            ]}
            style="margin-bottom: 1rem;"
        >
            <Toolbar><ToolbarContent /></Toolbar>
        </DataTable>
    </Row>
{/if}

<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>

<WalterLinks>
    <WalterLinkTile
        fileref={fileURL.vertrag(`${entry.vertrag?.id}`)}
        name={`Vertrag: ${entry?.vertrag?.text || 'ansehen'}`}
        href={`/vertraege/${entry.vertrag?.id}`}
    />

    <ClickableTile href={`/abrechnungslauf?jahr=${entry.jahr}`}>
        Zum Abrechnungslauf
    </ClickableTile>
</WalterLinks>
