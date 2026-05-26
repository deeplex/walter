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
        Button,
        Tag,
        InlineNotification
    } from 'carbon-components-svelte';
    import {
        WalterComboBox,
        WalterComboBoxWohnung,
        WalterNumberInput,
        WalterTextInput
    } from '$walter/components';
    import { walter_selection, walter_get } from '$walter/services/requests';
    import type {
        WalterSelectionEntry,
        WalterOffenerPostenStatus
    } from '$walter/lib';
    import type { ErhaltungsaufwendungsInput } from '$walter/lib';

    export let fetchImpl: typeof fetch;
    export let ea: ErhaltungsaufwendungsInput;
    export let availableBetrag = 0;
    export let isSinglePosition = false;
    export let invalid = false;

    let modeExisting = false;
    let wohnung: WalterSelectionEntry | undefined = undefined;
    let existingEa: WalterSelectionEntry | undefined = undefined;
    let eaInfo: WalterOffenerPostenStatus | undefined = undefined;

    const erhaltungsaufwendungen =
        walter_selection.erhaltungsaufwendungen(fetchImpl);

    $: ea.wohnungId = wohnung?.id as number | undefined;
    $: ea.existingBuchungssatzId = existingEa?.id as string | undefined;
    $: invalid = modeExisting
        ? !ea.existingBuchungssatzId
        : !ea.wohnungId;
    $: if (isSinglePosition && availableBetrag > 0) {
        ea.betrag = availableBetrag;
    }
    $: ladeEaInfo(existingEa?.id as string | undefined);

    async function ladeEaInfo(id: string | undefined) {
        if (!id) {
            eaInfo = undefined;
            return;
        }
        try {
            const resp = await walter_get(
                `/api/mietzahlungen/ea/${id}/info`,
                fetchImpl
            );
            if (resp && typeof resp === 'object' && 'rechnungsbetrag' in resp) {
                eaInfo = resp as WalterOffenerPostenStatus;
                if (isSinglePosition && eaInfo.verbleibenderBetrag > 0) {
                    ea.betrag = Math.min(
                        eaInfo.verbleibenderBetrag,
                        availableBetrag
                    );
                }
            }
        } catch {
            /* ignore */
        }
    }

    function switchToNew() {
        modeExisting = false;
        ea.existingBuchungssatzId = undefined;
        existingEa = undefined;
        eaInfo = undefined;
    }

    function switchToExisting() {
        modeExisting = true;
        ea.wohnungId = undefined;
        wohnung = undefined;
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
                Bestehende Forderung zahlen
            </Button>
        </div>
    </Column>
</Row>

{#if modeExisting}
    <Row>
        <Column>
            <WalterComboBox
                required
                titleText="Erhaltungsaufwendung"
                entries={erhaltungsaufwendungen}
                bind:value={existingEa}
            />
        </Column>
    </Row>
    {#if eaInfo}
        <Row>
            <Column>
                {#if eaInfo.verbleibenderBetrag <= 0}
                    <InlineNotification
                        kind="warning"
                        title="Bereits vollständig bezahlt"
                        subtitle="Rechnungsbetrag: {eaInfo.rechnungsbetrag.toFixed(2)} €, bereits gezahlt: {eaInfo.schonGezahlt.toFixed(2)} €"
                        hideCloseButton
                    />
                {:else}
                    <Tag type="blue">
                        Rechnungsbetrag: {eaInfo.rechnungsbetrag.toFixed(2)} € — noch offen: {eaInfo.verbleibenderBetrag.toFixed(2)} €
                    </Tag>
                    {#if eaInfo.schonGezahlt > 0}
                        <Tag type="green">
                            Bereits gezahlt: {eaInfo.schonGezahlt.toFixed(2)} €
                        </Tag>
                    {/if}
                    {#if ea.betrag > eaInfo.verbleibenderBetrag + 0.005}
                        <InlineNotification
                            kind="error"
                            title="Betrag zu hoch:"
                            subtitle="Maximal {eaInfo.verbleibenderBetrag.toFixed(2)} € offen"
                            hideCloseButton
                        />
                    {:else if ea.betrag > availableBetrag + 0.005}
                        <InlineNotification
                            kind="error"
                            title="Betrag übersteigt Transaktionsbetrag:"
                            subtitle="Maximal {availableBetrag.toFixed(2)} € verfügbar"
                            hideCloseButton
                        />
                    {:else if ea.betrag < eaInfo.verbleibenderBetrag - 0.005}
                        <InlineNotification
                            kind="warning"
                            title="Teilzahlung:"
                            subtitle="Noch {(eaInfo.verbleibenderBetrag - ea.betrag).toFixed(2)} € offen nach dieser Zahlung"
                            hideCloseButton
                        />
                    {/if}
                {/if}
            </Column>
        </Row>
    {/if}
    <Row>
        <Column>
            <WalterNumberInput
                required
                label="Betrag (€)"
                bind:value={ea.betrag}
            />
        </Column>
    </Row>
{:else}
    <Row>
        <Column>
            <WalterComboBoxWohnung required {fetchImpl} bind:value={wohnung} />
        </Column>
    </Row>
    <Row>
        <Column>
            <WalterNumberInput
                required
                label="Betrag (€)"
                bind:value={ea.betrag}
            />
        </Column>
        <Column>
            <WalterTextInput
                labelText="Beschreibung"
                bind:value={ea.beschreibung}
            />
        </Column>
    </Row>
{/if}
