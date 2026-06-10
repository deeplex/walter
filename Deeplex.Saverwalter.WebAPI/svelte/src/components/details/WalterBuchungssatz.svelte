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
    import type { DataTableRow } from 'carbon-components-svelte/types/DataTable/DataTable.svelte';

    import {
        WalterDataTable,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type { WalterBuchungssatzEntry } from '$walter/lib';
    import { convertEuro } from '$walter/services/utils';
    import { formatToTableDate } from '$walter/components/elements/WalterDataTable';
    import { Row } from 'carbon-components-svelte';

    export let entry: WalterBuchungssatzEntry;

    // Buchungssätze sind nach § 239 HGB unveränderlich — Korrekturen laufen
    // ausschließlich über Stornobuchungen, daher ist alles readonly.
    const zeilenHeaders = [
        { key: 'konto', value: 'Konto' },
        { key: 'soll', value: 'Soll' },
        { key: 'haben', value: 'Haben' },
        { key: 'opos', value: 'OPOS', sort: false as const }
    ];

    // OPOS-Status nur für Zeilen auf Ausgleichskonten — auf Summenkonten
    // (Erträge, Zahlungseingänge, ...) gibt es keine offenen Posten.
    function oposTag(zeile: (typeof entry.zeilen)[number]) {
        if (!zeile.ausgleichbar) {
            return '';
        }
        return zeile.offen <= 0.005
            ? { text: 'Ausgeglichen', tag: 'green' }
            : { text: `Offen: ${convertEuro(zeile.offen)}`, tag: 'red' };
    }

    $: zeilenRows = entry.zeilen.map((zeile) => ({
        id: zeile.id,
        kontoId: zeile.kontoId,
        konto: `${zeile.kontonummer} ${zeile.kontobezeichnung}`,
        soll: zeile.sollHaben === 'Soll' ? convertEuro(zeile.betrag) : '',
        haben: zeile.sollHaben === 'Haben' ? convertEuro(zeile.betrag) : '',
        opos: oposTag(zeile)
    }));

    const rowHref = (row: DataTableRow) => `/buchungskonten/${row.kontoId}`;
</script>

<Row>
    <WalterTextInput readonly value={entry.nummer} labelText="Buchungsnummer" />
    <WalterTextInput
        readonly
        value={formatToTableDate(entry.buchungsdatum)}
        labelText="Buchungsdatum"
    />
    {#if entry.status}
        <WalterTextInput readonly value={entry.status} labelText="Status" />
    {/if}
    <WalterTextInput
        readonly
        value={convertEuro(entry.betrag)}
        labelText="Betrag"
    />
</Row>
<Row>
    <WalterTextInput
        readonly
        value={entry.beschreibung}
        labelText="Beschreibung"
    />
</Row>
{#if entry.belegpfad}
    <Row>
        <WalterTextInput readonly value={entry.belegpfad} labelText="Beleg" />
    </Row>
{/if}
{#if entry.notiz}
    <Row>
        <WalterTextArea readonly value={entry.notiz} labelText="Notiz" />
    </Row>
{/if}

<WalterDataTable {rowHref} rows={zeilenRows} headers={zeilenHeaders} />
