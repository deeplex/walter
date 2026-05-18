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
        InlineNotification,
        Tag
    } from 'carbon-components-svelte';
    import {
        WalterComboBox,
        WalterMonthPicker,
        WalterNumberInput
    } from '$walter/components';
    import { walter_selection, walter_get } from '$walter/services/requests';
    import type { WalterSelectionEntry } from '$walter/lib';
    import type { WalterForderungsstatusEntry } from '$walter/lib';
    import type { MietzahlungsInput } from '$walter/lib';

    export let fetchImpl: typeof fetch;
    export let miete: MietzahlungsInput;
    export let availableBetrag = 0;
    export let isSinglePosition = false;

    let vertrag: WalterSelectionEntry | undefined = undefined;
    let forderungsstatus: WalterForderungsstatusEntry | undefined = undefined;

    const vertraege = walter_selection.vertraege(fetchImpl);

    const now = new Date();
    let betreffenderMonat: string = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-01`;
    miete.betreffenderMonat = betreffenderMonat;

    $: kaltmieteLabel = forderungsstatus?.grundmiete
        ? `Kaltmiete (€) — Grundmiete: ${forderungsstatus.grundmiete.toFixed(2)} € seit ${forderungsstatus.grundmieteSeit ?? '?'}`
        : 'Kaltmiete (€)';

    $: verteile(availableBetrag);

    // Distribute betrag across kaltmiete / NK using forderungsstatus (or grundmiete as fallback).
    function verteile(betrag: number) {
        if (!isSinglePosition) return;
        const kaltmieteSoll = forderungsstatus
            ? forderungsstatus.sollstellungVorhanden
                ? forderungsstatus.verbleibendeForderung
                : forderungsstatus.grundmiete
            : betrag;
        const kaltmiete = Math.min(betrag, Math.max(0, kaltmieteSoll));
        miete.kaltmiete = kaltmiete;
        miete.nkVorauszahlung = Math.max(0, betrag - kaltmiete);
    }

    async function ladeForderungsstatus() {
        const vertragId = miete.vertragId;
        const monat = miete.betreffenderMonat;
        if (!vertragId || !monat) return;
        try {
            const resp = await walter_get(
                `/api/mietzahlungen/${vertragId}/forderung/${monat}`,
                fetchImpl
            );
            if (resp && typeof resp === 'object' && 'monat' in resp) {
                forderungsstatus = resp as WalterForderungsstatusEntry;
            }

            if (isSinglePosition) verteile(availableBetrag);
        } catch {
            /* ignore */
        }
    }

    function onVertragSelect(e: CustomEvent) {
        vertrag = e.detail?.selectedItem;
        miete.vertragId = vertrag?.id as number | undefined;
        forderungsstatus = undefined;
        ladeForderungsstatus();
    }

    $: if (betreffenderMonat !== undefined) {
        miete.betreffenderMonat = betreffenderMonat;
        ladeForderungsstatus();
    }
</script>

<Row>
    <Column>
        <WalterComboBox
            required
            titleText="Vertrag"
            entries={vertraege}
            bind:value={vertrag}
            initialId={miete.vertragId}
            on:select={onVertragSelect}
        />
    </Column>
    <Column>
        <WalterMonthPicker
            required
            labelText="Betreffender Monat"
            bind:value={betreffenderMonat}
        />
    </Column>
</Row>

<Row>
    <Column>
        <WalterNumberInput
            required
            label={kaltmieteLabel}
            bind:value={miete.kaltmiete}
        />
    </Column>
    <Column>
        <WalterNumberInput
            label="NK-Vorauszahlung (€)"
            bind:value={miete.nkVorauszahlung}
        />
    </Column>
</Row>

{#if forderungsstatus}
    <Row>
        <Column>
            {#if forderungsstatus.sollstellungVorhanden}
                {@const erwartet =
                    forderungsstatus.verbleibendeForderung +
                    forderungsstatus.nkVorauszahlung}
                {@const aktuell =
                    (miete.kaltmiete || 0) + (miete.nkVorauszahlung || 0)}
                <Tag type="green">
                    Forderung: {forderungsstatus.forderungsbetrag.toFixed(2)} € —
                    noch offen: {forderungsstatus.verbleibendeForderung.toFixed(
                        2
                    )} €
                </Tag>
                {#if Math.abs(aktuell - erwartet) >= 0.005}
                    <InlineNotification
                        kind="warning"
                        title="Betrag weicht von Forderung ab:"
                        subtitle="Erwartet {erwartet.toFixed(
                            2
                        )} €, eingetragen {aktuell.toFixed(2)} €"
                        hideCloseButton
                    />
                {/if}
            {:else}
                <InlineNotification
                    kind="info"
                    title="Keine Sollstellung"
                    subtitle="Für diesen Monat liegt noch keine Sollstellung vor. Sie wird automatisch erstellt."
                    hideCloseButton
                />
            {/if}
        </Column>
    </Row>
{/if}
