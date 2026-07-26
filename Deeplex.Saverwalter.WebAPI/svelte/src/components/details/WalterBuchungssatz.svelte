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
        WalterNumberInput,
        WalterTextArea,
        WalterTextInput
    } from '$walter/components';
    import type { WalterBuchungssatzEntry } from '$walter/lib';
    import { convertEuro } from '$walter/services/utils';
    import { formatToTableDate } from '$walter/components/elements/WalterDataTable';
    import { Column, Row, Tag } from 'carbon-components-svelte';

    export let entry: WalterBuchungssatzEntry;

    // Der Betrag jeder Buchungszeile ist direkt editierbar (Speichern-Button) —
    // Korrekturen überschreiben damit den ursprünglichen Wert, statt über eine
    // Stornobuchung nachvollziehbar zu bleiben. Bewusste Entscheidung des Nutzers,
    // daher an dieselbe Regel wie Löschen gekoppelt: nur "freie" Sätze (kannLoeschen)
    // sind editierbar, alles andere geht nur über Stornieren (vom Backend erzwungen).

    // OPOS-Status nur für Zeilen auf Ausgleichskonten — auf Summenkonten
    // (Erträge, Zahlungseingänge, ...) gibt es keine offenen Posten.
    function oposTag(zeile: (typeof entry.zeilen)[number]) {
        if (!zeile.ausgleichbar) {
            return null;
        }
        return zeile.offen <= 0.005
            ? { text: 'Ausgeglichen', type: 'green' as const }
            : {
                  text: `Offen: ${convertEuro(zeile.offen)}`,
                  type: 'red' as const
              };
    }
</script>

<Row>
    <WalterTextInput readonly value={entry.nummer} labelText="Buchungsnummer" />
    <WalterNumberInput
        bind:value={entry.buchungsjahr}
        label="Buchungsjahr"
        min={2000}
        max={2100}
    />
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
    <WalterTextInput bind:value={entry.beschreibung} labelText="Beschreibung" />
</Row>
{#if entry.belegpfad}
    <Row>
        <WalterTextInput readonly value={entry.belegpfad} labelText="Beleg" />
    </Row>
{/if}
<Row>
    <WalterTextArea bind:value={entry.notiz} labelText="Notiz" />
</Row>

{#each entry.zeilen as zeile (zeile.id)}
    <Row>
        <Column>
            <WalterTextInput
                readonly
                value={`${zeile.kontonummer} ${zeile.kontobezeichnung}`}
                labelText="Konto"
            />
        </Column>
        <Column>
            <WalterNumberInput
                bind:value={zeile.betrag}
                label={zeile.sollHaben === 'Soll' ? 'Soll' : 'Haben'}
                min={0}
                readonly={!entry.kannLoeschen}
            />
        </Column>
        <Column>
            {#if oposTag(zeile)}
                <Tag type={oposTag(zeile)?.type}>{oposTag(zeile)?.text}</Tag>
            {/if}
        </Column>
    </Row>
{/each}
