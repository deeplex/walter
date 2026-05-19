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
    import { Row, Column, Button, Tag, InlineNotification } from 'carbon-components-svelte';
    import {
        WalterComboBox,
        WalterDatePicker,
        WalterNumberInput
    } from '$walter/components';
    import { walter_selection, walter_get } from '$walter/services/requests';
    import type { WalterSelectionEntry, WalterOffenerPostenStatus } from '$walter/lib';
    import type { BetriebskostenEingangInput } from '$walter/lib';

    export let fetchImpl: typeof fetch;
    export let bk: BetriebskostenEingangInput;
    export let availableBetrag = 0;
    export let isSinglePosition = false;

    let modeExisting = false;
    let umlage: WalterSelectionEntry | undefined = undefined;
    let existingBk: WalterSelectionEntry | undefined = undefined;
    let offenerPosten: WalterOffenerPostenStatus | undefined = undefined;

    const umlagen = walter_selection.umlagen(fetchImpl);
    const betriebskostenrechnungen =
        walter_selection.betriebskostenrechnungen(fetchImpl);

    if (!bk.betreffendesJahr) {
        bk.betreffendesJahr = new Date().getFullYear() - 1;
    }

    $: bk.umlageId = umlage?.id as number | undefined;
    $: bk.existingBetriebskostenrechnungId = existingBk?.id as
        | number
        | undefined;
    $: if (isSinglePosition && availableBetrag > 0) {
        bk.betrag = availableBetrag;
    }
    $: ladeOffenerPosten(existingBk?.id as number | undefined);

    async function ladeOffenerPosten(id: number | undefined) {
        if (!id) {
            offenerPosten = undefined;
            return;
        }
        try {
            const resp = await walter_get(
                `/api/mietzahlungen/bk/${id}/offenerposten`,
                fetchImpl
            );
            if (resp && typeof resp === 'object' && 'rechnungsbetrag' in resp) {
                offenerPosten = resp as WalterOffenerPostenStatus;
                if (isSinglePosition && offenerPosten.verbleibenderBetrag > 0) {
                    bk.betrag = offenerPosten.verbleibenderBetrag;
                }
            }
        } catch {
            /* ignore */
        }
    }

    function switchToNew() {
        modeExisting = false;
        bk.existingBetriebskostenrechnungId = undefined;
        existingBk = undefined;
        offenerPosten = undefined;
    }

    function switchToExisting() {
        modeExisting = true;
        bk.umlageId = undefined;
        umlage = undefined;
    }
</script>

<Row style="margin-bottom: 0.5rem">
    <Column>
        <div style="display: flex; gap: 0.5rem">
            <Button
                kind={!modeExisting ? 'primary' : 'ghost'}
                size="small"
                on:click={switchToNew}
            >
                Neu anlegen
            </Button>
            <Button
                kind={modeExisting ? 'primary' : 'ghost'}
                size="small"
                on:click={switchToExisting}
            >
                Bestehend verknüpfen
            </Button>
        </div>
    </Column>
</Row>

{#if modeExisting}
    <Row>
        <Column>
            <WalterComboBox
                required
                titleText="Betriebskostenrechnung"
                entries={betriebskostenrechnungen}
                bind:value={existingBk}
            />
        </Column>
        <Column>
            <WalterNumberInput
                required
                label="Betrag (€)"
                bind:value={bk.betrag}
            />
        </Column>
    </Row>
    {#if offenerPosten}
        <Row>
            <Column>
                {#if offenerPosten.verbleibenderBetrag <= 0}
                    <InlineNotification
                        kind="warning"
                        title="Bereits vollständig bezahlt"
                        subtitle="Rechnungsbetrag: {offenerPosten.rechnungsbetrag.toFixed(2)} €, bereits gezahlt: {offenerPosten.schonGezahlt.toFixed(2)} €"
                        hideCloseButton
                    />
                {:else}
                    <Tag type="blue">
                        Rechnungsbetrag: {offenerPosten.rechnungsbetrag.toFixed(2)} € —
                        noch offen: {offenerPosten.verbleibenderBetrag.toFixed(2)} €
                    </Tag>
                    {#if offenerPosten.schonGezahlt > 0}
                        <Tag type="green">
                            Bereits gezahlt: {offenerPosten.schonGezahlt.toFixed(2)} €
                        </Tag>
                    {/if}
                {/if}
            </Column>
        </Row>
    {/if}
{:else}
    <Row>
        <Column>
            <WalterComboBox
                required
                titleText="Umlage"
                entries={umlagen}
                bind:value={umlage}
                initialId={bk.umlageId}
            />
        </Column>
        <Column>
            <WalterNumberInput
                required
                label="Betreffendes Jahr"
                digits={0}
                bind:value={bk.betreffendesJahr}
            />
        </Column>
    </Row>
    <Row>
        <Column>
            <WalterDatePicker
                required
                labelText="Rechnungsdatum"
                bind:value={bk.rechnungsDatum}
            />
        </Column>
        <Column>
            <WalterNumberInput
                required
                label="Betrag (€)"
                bind:value={bk.betrag}
            />
        </Column>
    </Row>
{/if}
