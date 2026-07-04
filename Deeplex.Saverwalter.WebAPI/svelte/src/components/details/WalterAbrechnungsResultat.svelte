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
    import { WalterLinks, WalterTransaktion } from '$walter/components';
    import {
        Button,
        Checkbox,
        ClickableTile,
        DataTable,
        InlineNotification,
        Row,
        Column,
        Tag,
        Tile
    } from 'carbon-components-svelte';
    import { Add } from 'carbon-icons-svelte';
    import { invalidateAll } from '$app/navigation';
    import WalterLinkTile from '../subdetails/WalterLinkTile.svelte';
    import WalterDataWrapperQuickAdd from '../elements/WalterDataWrapperQuickAdd.svelte';
    import { fileURL } from '$walter/services/files';
    import type { WalterAbrechnungsresultatEntry } from '$walter/lib/WalterAbrechnungsresultat';
    import WalterTextArea from '../elements/WalterTextArea.svelte';
    import { convertEuro } from '$walter/services/utils';
    import {
        emptyTransaktionsInput,
        type TransaktionsInput
    } from '$walter/lib';

    export let entry: WalterAbrechnungsresultatEntry;
    export let fetchImpl: typeof fetch;

    export let readonly = false;
    $: {
        readonly =
            entry?.permissions?.update === false || entry?.abgesendet === true;
    }

    const abgesendet = (e: Event) => {
        entry.abgesendet = (e.target as HTMLInputElement).checked;
    };

    const formatDate = (d: string) => {
        const [y, m, day] = d.split('-');
        return `${day}.${m}.${y}`;
    };

    // Ausgleich als Transaktion erfassen — vorausgefüllt mit diesem Resultat.
    let ausgleichModalOpen = false;
    let ausgleichInput: TransaktionsInput = emptyTransaktionsInput();
    let transaktionValid = false;

    function openAusgleichModal() {
        ausgleichInput = {
            ...emptyTransaktionsInput(),
            betrag: entry.offenerBetrag,
            verwendungszweck:
                `BK-Abrechnung ${entry.jahr} ${entry.vertrag?.text ?? ''}`.trim(),
            // Vermieter-Bankkonto: bei Erstattung fließt das Geld davon ab,
            // bei Nachzahlung kommt es dort an.
            zahlerId: entry.saldo < 0 ? entry.vermieterBankkontoId : undefined,
            zahlungsempfaengerId:
                entry.saldo > 0 ? entry.vermieterBankkontoId : undefined,
            abrechnungsAusgleiche: [
                {
                    abrechnungsresultatId: entry.id,
                    betrag: entry.offenerBetrag
                }
            ]
        };
        ausgleichModalOpen = true;
    }
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

<!-- Abrechnungs-Übersicht -->
<Row style="margin-bottom: 1rem">
    <Column>
        <Tile>
            <h4 style="margin-bottom: 0.75rem">
                Betriebskostenabrechnung {entry.jahr}
                {#if entry.abgesendet}
                    <Tag type="blue">Abgesendet</Tag>
                {:else}
                    <Tag type="gray">Entwurf</Tag>
                {/if}
                {#if entry.ausgeglichen}
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
            {#if entry.abgesendet && !entry.ausgeglichen && entry.saldo !== 0}
                <Button
                    kind="tertiary"
                    size="small"
                    icon={Add}
                    style="margin-top: 0.5rem"
                    on:click={openAusgleichModal}
                >
                    Ausgleich als Transaktion erfassen
                </Button>
            {/if}
        </Tile>
    </Column>
</Row>

<WalterDataWrapperQuickAdd
    title="Ausgleich BK-Abrechnung {entry.jahr}"
    addUrl="/api/transaktionen/buchen"
    bind:addEntry={ausgleichInput}
    bind:addModalOpen={ausgleichModalOpen}
    submitDisabled={!transaktionValid}
    onSubmit={() => invalidateAll()}
>
    <WalterTransaktion
        {fetchImpl}
        bind:buchung={ausgleichInput}
        bind:isValid={transaktionValid}
    />
</WalterDataWrapperQuickAdd>

<!-- Ausgleichszahlungen -->
{#if entry.ausgleichsZahlungen?.length > 0}
    <Row>
        <DataTable
            title="Ausgleichszahlungen"
            size="short"
            headers={[
                { key: 'datum', value: 'Datum' },
                { key: 'betrag', value: 'Betrag' },
                { key: 'link', value: '' }
            ]}
            rows={entry.ausgleichsZahlungen.map((z, i) => ({
                id: String(i),
                datum: formatDate(z.datum),
                betrag: convertEuro(z.betrag),
                link: z.buchungssatzId
            }))}
            style="margin-bottom: 1rem;"
        >
            <svelte:fragment slot="cell" let:cell>
                {#if cell.key === 'link'}
                    <a href={`/buchungssaetze/${cell.value}`}
                        >Zum Buchungssatz</a
                    >
                {:else}
                    {cell.value}
                {/if}
            </svelte:fragment>
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
