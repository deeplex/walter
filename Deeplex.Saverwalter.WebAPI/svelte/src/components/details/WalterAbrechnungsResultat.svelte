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
    import WalterDataTable from '$walter/components/elements/WalterDataTable.svelte';
    import { WalterLinks } from '$walter/components';
    import WalterZahlungErfassen from './WalterZahlungErfassen.svelte';
    import {
        Button,
        Checkbox,
        ClickableTile,
        InlineNotification,
        Row,
        Column,
        Tag,
        Tile
    } from 'carbon-components-svelte';
    import { Add } from 'carbon-icons-svelte';
    import { invalidateAll } from '$app/navigation';
    import WalterLinkTile from '../subdetails/WalterLinkTile.svelte';
    import type { WalterAbrechnungsresultatEntry } from '$walter/lib/WalterAbrechnungsresultat';
    import WalterTextArea from '../elements/WalterTextArea.svelte';
    import {
        convertEuro,
        walter_subscribe_reset_changeTracker
    } from '$walter/services/utils';

    export let entry: WalterAbrechnungsresultatEntry;
    export let fetchImpl: typeof fetch;

    export let readonly = false;
    $: {
        readonly =
            entry?.permissions?.update === false ||
            entry?.abgesendet === true ||
            entry?.storniert === true;
    }

    // Persistierter (in der DB gespeicherter) Abgesendet-Zustand — NICHT der bloß
    // lokal angehakte. Der Ausgleich darf erst nach dem Speichern erfasst werden.
    let abgesendetPersistiert = entry?.abgesendet === true;
    walter_subscribe_reset_changeTracker(() => {
        abgesendetPersistiert = entry?.abgesendet === true;
    });

    const abgesendet = (e: Event) => {
        entry.abgesendet = (e.target as HTMLInputElement).checked;
    };

    // Nachzahlung bzw. Erstattung erfassen.
    let ausgleichModalOpen = false;
    $: erstattung = entry.saldo < 0;
</script>

{#if entry.storniert}
    <Row>
        <InlineNotification
            kind="warning"
            title="Storniert:"
            subtitle="Diese Abrechnung wurde storniert und gilt nicht mehr. Sie bleibt als Beleg erhalten; das Jahr kann im Abrechnungslauf neu abgerechnet werden."
            hideCloseButton
        />
    </Row>
{:else if entry.abgesendet}
    <Row>
        <InlineNotification
            kind="info"
            title="Abgesendet:"
            subtitle="Diese Abrechnung wurde versendet und ist gesperrt. Zum Ändern bitte zuerst stornieren."
            hideCloseButton
        />
    </Row>
{/if}

<!-- Abrechnungs-Übersicht -->
<Row style="margin-bottom: 1rem">
    <Column>
        <Tile>
            <h4 style="margin-bottom: 0.75rem">
                Betriebskostenabrechnung {entry.jahr}
                {#if entry.storniert}
                    <Tag type="red">Storniert</Tag>
                {:else if entry.abgesendet}
                    <Tag type="blue">Abgesendet</Tag>
                {:else}
                    <Tag type="gray">Entwurf</Tag>
                {/if}
                {#if entry.storniert}
                    <!-- kein offener Betrag: die Forderung ist storniert -->
                {:else if entry.ausgeglichen}
                    <Tag type="green">Ausgeglichen</Tag>
                {:else}
                    <Tag type="red"
                        >Offen: {convertEuro(entry.offenerBetrag)}</Tag
                    >
                {/if}
            </h4>
            <div style="display: flex; gap: 3rem; flex-wrap: wrap">
                <div>
                    <div style="font-size: 0.75rem; opacity: 0.7">
                        Rechnungsbetrag
                    </div>
                    <strong>{convertEuro(entry.rechnungsbetrag)}</strong>
                </div>
                <div>
                    <div style="font-size: 0.75rem; opacity: 0.7">
                        NK-Vorauszahlungen
                    </div>
                    <strong>{convertEuro(entry.vorauszahlung)}</strong>
                </div>
                <div>
                    <div style="font-size: 0.75rem; opacity: 0.7">Saldo</div>
                    <strong>
                        {convertEuro(Math.abs(entry.saldo))}
                        {#if entry.saldo > 0}
                            <Tag size="sm" type="red"
                                >Nachzahlung des Mieters</Tag
                            >
                        {:else if entry.saldo < 0}
                            <Tag size="sm" type="teal"
                                >Erstattung an den Mieter</Tag
                            >
                        {/if}
                    </strong>
                </div>
            </div>
        </Tile>
    </Column>
    <Column>
        <Tile light style="height: 100%">
            <Checkbox
                disabled={readonly}
                labelText="Ist diese Abrechnung an den Mieter versendet?"
                bind:checked={entry.abgesendet}
                on:change={abgesendet}
            />
            {#if abgesendetPersistiert && !entry.storniert && !entry.ausgeglichen && entry.saldo !== 0}
                <Button
                    kind="tertiary"
                    size="small"
                    icon={Add}
                    style="margin-top: 0.5rem"
                    on:click={() => (ausgleichModalOpen = true)}
                >
                    {erstattung ? 'Erstattung' : 'Nachzahlung'} erfassen
                </Button>
            {:else if entry.abgesendet && !abgesendetPersistiert && !entry.ausgeglichen && entry.saldo !== 0}
                <p
                    style="margin-top: 0.5rem; font-size: 0.75rem; color: var(--cds-text-secondary);"
                >
                    Zum Erfassen des Ausgleichs bitte zuerst speichern.
                </p>
            {/if}
        </Tile>
    </Column>
</Row>

<WalterZahlungErfassen
    {fetchImpl}
    ziel={{ art: 'abrechnung', abrechnungsresultatId: entry.id, erstattung }}
    titel={`${erstattung ? 'Erstattung' : 'Nachzahlung'} Abrechnung ${entry.jahr}`}
    vorschlag={entry.offenerBetrag}
    vermieterBankkontoId={entry.vermieterBankkontoId ?? undefined}
    bind:open={ausgleichModalOpen}
    on:gebucht={() => invalidateAll()}
/>

<!-- Ausgleichszahlungen -->
{#if entry.ausgleichsZahlungen?.length > 0}
    <Row>
        <WalterDataTable
            title="Ausgleichszahlungen"
            size="short"
            search={false}
            headers={[
                { key: 'datum', value: 'Datum' },
                { key: 'betrag', value: 'Betrag' },
                { key: 'link', value: '' }
            ]}
            rows={entry.ausgleichsZahlungen.map((z, i) => ({
                id: String(i),
                datum: z.datum,
                betrag: z.betrag,
                link: z.buchungssatzId
            }))}
            customCells={['link']}
        >
            <svelte:fragment slot="cell" let:cell>
                {#if cell.key === 'link'}
                    <a href={`/buchungssaetze/${cell.value}`}
                        >Zum Buchungssatz</a
                    >
                {/if}
            </svelte:fragment>
        </WalterDataTable>
    </Row>
{/if}

<Row>
    <WalterTextArea {readonly} bind:value={entry.notiz} labelText="Notiz" />
</Row>

<WalterLinks>
    <WalterLinkTile
        name={`Vertrag: ${entry?.vertrag?.text || 'ansehen'}`}
        href={`/vertraege/${entry.vertrag?.id}`}
    />

    <ClickableTile href={`/abrechnungslauf?jahr=${entry.jahr}`}>
        Zum Abrechnungslauf
    </ClickableTile>
</WalterLinks>
