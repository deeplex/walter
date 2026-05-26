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
    import { createEventDispatcher, onMount } from 'svelte';
    import {
        Row,
        Column,
        Button,
        Tag,
        InlineNotification
    } from 'carbon-components-svelte';
    import {
        WalterComboBox,
        WalterDatePicker,
        WalterNumberInput
    } from '$walter/components';
    import { walter_selection, walter_get } from '$walter/services/requests';
    import type {
        WalterSelectionEntry,
        WalterOffenerPostenStatus
    } from '$walter/lib';
    import type { BetriebskostenEingangInput } from '$walter/lib';

    const dispatch = createEventDispatcher<{
        zahlerResolved: number | undefined;
    }>();

    export let fetchImpl: typeof fetch;
    export let bk: BetriebskostenEingangInput;
    export let availableBetrag = 0;
    export let isSinglePosition = false;
    export let invalid = false;

    let modeExisting = false;
    let umlage: WalterSelectionEntry | undefined = undefined;
    let existingBk: WalterSelectionEntry | undefined = undefined;
    let offenerPosten: WalterOffenerPostenStatus | undefined = undefined;

    const umlagen = walter_selection.umlagen(fetchImpl);
    const betriebskostenrechnungen =
        walter_selection.betriebskostenrechnungenOffen(fetchImpl);

    if (!bk.betreffendesJahr) {
        bk.betreffendesJahr = new Date().getFullYear() - 1;
    }

    onMount(async () => {
        if (bk.existingBuchungssatzId) {
            modeExisting = true;
            autoSelectBkId = bk.existingBuchungssatzId;
            await ladeOffenerPosten(bk.existingBuchungssatzId);
            try {
                const resp = await walter_get(
                    `/api/selection/zahler-bankkonto/buchungssatz/${bk.existingBuchungssatzId}`,
                    fetchImpl
                );
                dispatch(
                    'zahlerResolved',
                    (resp as WalterSelectionEntry | null)?.id as
                        | number
                        | undefined
                );
            } catch {
                dispatch('zahlerResolved', undefined);
            }
        } else if (bk.umlageId) {
            resolveAndDispatchZahler(bk.umlageId);
        }
    });

    async function resolveAndDispatchZahler(umlageId: number | undefined) {
        if (!umlageId) {
            dispatch('zahlerResolved', undefined);
            return;
        }
        try {
            const resp = await walter_get(
                `/api/selection/zahler-bankkonto/umlage/${umlageId}`,
                fetchImpl
            );
            dispatch(
                'zahlerResolved',
                (resp as WalterSelectionEntry | null)?.id as number | undefined
            );
        } catch {
            dispatch('zahlerResolved', undefined);
        }
    }

    function onUmlageSelect(e: CustomEvent) {
        umlage = e.detail?.selectedItem;
        bk.umlageId = umlage?.id as number | undefined;
        resolveAndDispatchZahler(bk.umlageId);
    }

    async function onExistingBkSelect(e: CustomEvent) {
        existingBk = e.detail?.selectedItem;
        bk.existingBuchungssatzId = existingBk?.id as string | undefined;
        ladeOffenerPosten(existingBk?.id as string | undefined);
        if (existingBk?.id) {
            try {
                const resp = await walter_get(
                    `/api/selection/zahler-bankkonto/buchungssatz/${existingBk.id}`,
                    fetchImpl
                );
                dispatch(
                    'zahlerResolved',
                    (resp as WalterSelectionEntry | null)?.id as
                        | number
                        | undefined
                );
            } catch {
                dispatch('zahlerResolved', undefined);
            }
        } else {
            dispatch('zahlerResolved', undefined);
        }
    }

    type BkForderungInfo = {
        buchungssatzId: string;
        betrag: number;
        datum: string;
        schonGezahlt: number;
        verbleibend: number;
    };
    let existingeForderungen: BkForderungInfo[] = [];
    $: if (!modeExisting && bk.umlageId && bk.betreffendesJahr) {
        walter_get(
            `/api/mietzahlungen/bk/check-forderung?umlageId=${bk.umlageId}&year=${bk.betreffendesJahr}`,
            fetchImpl
        )
            .then((r) => {
                existingeForderungen = (r as BkForderungInfo[]) ?? [];
            })
            .catch(() => {
                existingeForderungen = [];
            });
    } else if (modeExisting) {
        existingeForderungen = [];
    }

    function forderungStatus(f: BkForderungInfo): string {
        if (f.verbleibend <= 0) return 'vollständig bezahlt';
        if (f.schonGezahlt > 0)
            return `${f.schonGezahlt.toFixed(2)} € bereits gezahlt, ${f.verbleibend.toFixed(2)} € offen`;
        return 'noch nicht bezahlt';
    }

    $: invalid = modeExisting
        ? !bk.existingBuchungssatzId ||
          !!(
              (offenerPosten &&
                  bk.betrag > offenerPosten.verbleibenderBetrag + 0.005) ||
              bk.betrag > availableBetrag + 0.005
          )
        : !bk.umlageId || !bk.rechnungsDatum || !bk.betreffendesJahr;

    $: if (isSinglePosition && availableBetrag > 0) {
        bk.betrag = availableBetrag;
    }

    async function ladeOffenerPosten(id: string | undefined) {
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
                    bk.betrag = Math.min(
                        offenerPosten.verbleibenderBetrag,
                        availableBetrag
                    );
                }
            }
        } catch {
            /* ignore */
        }
    }

    let autoSelectBkId: string | undefined = undefined;

    function switchToNew() {
        modeExisting = false;
        bk.existingBuchungssatzId = undefined;
    }

    async function switchToExisting() {
        const first = existingeForderungen[0];
        modeExisting = true;
        if (first && !bk.existingBuchungssatzId) {
            autoSelectBkId = first.buchungssatzId;
            bk.existingBuchungssatzId = first.buchungssatzId;
            ladeOffenerPosten(first.buchungssatzId);
            try {
                const resp = await walter_get(
                    `/api/selection/zahler-bankkonto/buchungssatz/${first.buchungssatzId}`,
                    fetchImpl
                );
                dispatch(
                    'zahlerResolved',
                    (resp as WalterSelectionEntry | null)?.id as
                        | number
                        | undefined
                );
            } catch {
                dispatch('zahlerResolved', undefined);
            }
        }
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
                on:click={() => switchToExisting()}
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
                titleText="Betriebskostenrechnung"
                entries={betriebskostenrechnungen}
                bind:value={existingBk}
                initialId={autoSelectBkId}
                on:select={onExistingBkSelect}
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
                        subtitle="Rechnungsbetrag: {offenerPosten.rechnungsbetrag.toFixed(
                            2
                        )} €, bereits gezahlt: {offenerPosten.schonGezahlt.toFixed(
                            2
                        )} €"
                        hideCloseButton
                    />
                {:else}
                    <Tag type="blue">
                        Rechnungsbetrag: {offenerPosten.rechnungsbetrag.toFixed(
                            2
                        )} € — noch offen: {offenerPosten.verbleibenderBetrag.toFixed(
                            2
                        )} €
                    </Tag>
                    {#if offenerPosten.schonGezahlt > 0}
                        <Tag type="green">
                            Bereits gezahlt: {offenerPosten.schonGezahlt.toFixed(
                                2
                            )} €
                        </Tag>
                    {/if}
                    {#if bk.betrag > offenerPosten.verbleibenderBetrag + 0.005}
                        <InlineNotification
                            kind="error"
                            title="Betrag zu hoch:"
                            subtitle="Maximal {offenerPosten.verbleibenderBetrag.toFixed(
                                2
                            )} € offen"
                            hideCloseButton
                        />
                    {:else if bk.betrag > availableBetrag + 0.005}
                        <InlineNotification
                            kind="error"
                            title="Betrag übersteigt Transaktionsbetrag:"
                            subtitle="Maximal {availableBetrag.toFixed(
                                2
                            )} € verfügbar"
                            hideCloseButton
                        />
                    {:else if bk.betrag < offenerPosten.verbleibenderBetrag - 0.005}
                        <InlineNotification
                            kind="warning"
                            title="Teilzahlung:"
                            subtitle="Noch {(
                                offenerPosten.verbleibenderBetrag - bk.betrag
                            ).toFixed(2)} € offen nach dieser Zahlung"
                            hideCloseButton
                        />
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
                on:select={onUmlageSelect}
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
    {#if existingeForderungen.length > 0}
        <Row>
            <Column>
                <InlineNotification
                    kind="warning"
                    title="Bereits {existingeForderungen.length} Forderung{existingeForderungen.length >
                    1
                        ? 'en'
                        : ''} für dieses Jahr vorhanden"
                    subtitle={existingeForderungen
                        .map(
                            (f) =>
                                `${f.betrag.toFixed(2)} € vom ${new Date(f.datum).toLocaleDateString('de-DE')} (${forderungStatus(f)})`
                        )
                        .join(' | ')}
                    hideCloseButton
                />
            </Column>
        </Row>
    {/if}
{/if}
