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
        InlineLoading
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
    export let invalid = false;
    export let statusText = '';

    let isLoading = false;
    let vertrag: WalterSelectionEntry | undefined = undefined;
    let forderungsstatus: WalterForderungsstatusEntry | undefined = undefined;

    const vertraege = walter_selection.vertraege(fetchImpl);

    const now = new Date();
    let betreffenderMonat: string = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-01`;
    miete.betreffenderMonat = betreffenderMonat;

    $: statusText = forderungsstatus?.sollstellungVorhanden
        ? `Bereits gezahlt: ${forderungsstatus.schonGezahlt.toFixed(2)} € — Verbleibend: ${forderungsstatus.verbleibendeForderung.toFixed(2)} €`
        : '';

    $: kaltmieteLabel = forderungsstatus?.grundmiete
        ? forderungsstatus.sollstellungVorhanden
            ? `Kaltmiete (€) — Grundmiete: ${forderungsstatus.grundmiete.toFixed(2)} € — Verbleibend: ${forderungsstatus.verbleibendeForderung.toFixed(2)} €`
            : `Kaltmiete (€) — Grundmiete: ${forderungsstatus.grundmiete.toFixed(2)} €`
        : 'Kaltmiete (€)';

    $: kaltmieteMax = forderungsstatus
        ? forderungsstatus.sollstellungVorhanden
            ? forderungsstatus.verbleibendeForderung
            : forderungsstatus.grundmiete
        : undefined;

    $: invalid =
        !miete.vertragId ||
        !!(
            kaltmieteMax !== undefined && miete.kaltmiete > kaltmieteMax + 0.005
        );

    $: verteile(availableBetrag);

    // Distribute betrag: Kaltmiete → Garagenmieten → NK-VZ
    function verteile(betrag: number) {
        if (!isSinglePosition) return;

        const kaltmieteSoll = forderungsstatus
            ? forderungsstatus.sollstellungVorhanden
                ? forderungsstatus.verbleibendeForderung
                : forderungsstatus.grundmiete
            : betrag;

        let remaining = betrag;
        miete.kaltmiete = Math.min(remaining, Math.max(0, kaltmieteSoll));
        remaining = Math.max(0, remaining - miete.kaltmiete);

        if (miete.garagen && forderungsstatus?.garagen) {
            for (const g of miete.garagen) {
                const gStatus = forderungsstatus.garagen.find(
                    (fs) => fs.garageVertragId === g.garageVertragId
                );
                const soll = gStatus
                    ? gStatus.sollstellungVorhanden
                        ? gStatus.verbleibendeForderung
                        : gStatus.garagenMiete
                    : g.betrag;
                g.betrag = Math.min(remaining, Math.max(0, soll));
                remaining = Math.max(0, remaining - g.betrag);
            }
        }

        miete.nkVorauszahlung = Math.max(0, remaining);
    }

    async function ladeForderungsstatus() {
        const vertragId = miete.vertragId;
        const monat = miete.betreffenderMonat;
        if (!vertragId || !monat) return;
        isLoading = true;
        try {
            const resp = await walter_get(
                `/api/mietzahlungen/${vertragId}/forderung/${monat}`,
                fetchImpl
            );
            if (resp && typeof resp === 'object' && 'monat' in resp) {
                forderungsstatus = resp as WalterForderungsstatusEntry;

                // Sync garage entries with the status from backend
                miete.garagen = (forderungsstatus.garagen ?? []).map((g) => ({
                    garageVertragId: g.garageVertragId,
                    garageKennung: g.garageKennung,
                    betrag: g.sollstellungVorhanden
                        ? g.verbleibendeForderung
                        : g.garagenMiete
                }));
            }

            if (isSinglePosition) verteile(availableBetrag);
        } catch {
            /* ignore */
        } finally {
            isLoading = false;
        }
    }

    function onVertragSelect(e: CustomEvent) {
        vertrag = e.detail?.selectedItem;
        miete.vertragId = vertrag?.id as number | undefined;
        miete.garagen = [];
        forderungsstatus = undefined;
        ladeForderungsstatus();
    }

    $: if (betreffenderMonat !== undefined) {
        miete.betreffenderMonat = betreffenderMonat;
        ladeForderungsstatus();
    }

    function garageLabel(
        g: { garageKennung: string; garageVertragId: number },
        status: WalterForderungsstatusEntry | undefined
    ) {
        const gst = status?.garagen?.find(
            (s) => s.garageVertragId === g.garageVertragId
        );
        if (!gst) return `Garagenmiete ${g.garageKennung} (€)`;
        const hint = gst.sollstellungVorhanden
            ? `Grundmiete: ${gst.garagenMiete.toFixed(2)} € | Verbleibend: ${gst.verbleibendeForderung.toFixed(2)} €`
            : `Grundmiete: ${gst.garagenMiete.toFixed(2)} €`;
        return `Garagenmiete ${g.garageKennung} (€) — ${hint}`;
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

{#if isLoading}
    <Row>
        <Column>
            <InlineLoading description="Garagenmieten werden geladen…" />
        </Column>
    </Row>
{:else}
    {#each miete.garagen ?? [] as garage}
        <Row>
            <Column>
                <WalterNumberInput
                    label={garageLabel(garage, forderungsstatus)}
                    bind:value={garage.betrag}
                />
            </Column>
        </Row>
    {/each}
{/if}

{#if forderungsstatus}
    <Row>
        <Column>
            {#if forderungsstatus.sollstellungVorhanden && invalid}
                <InlineNotification
                    kind="error"
                    title="Kaltmiete zu hoch:"
                    subtitle="Maximal {kaltmieteMax?.toFixed(2)} € erlaubt"
                    hideCloseButton
                />
            {:else if forderungsstatus.sollstellungVorhanden && miete.kaltmiete < forderungsstatus.verbleibendeForderung - 0.005}
                <InlineNotification
                    kind="warning"
                    title="Betrag liegt unter geforderter Kaltmiete:"
                    subtitle="Erwartet {forderungsstatus.verbleibendeForderung.toFixed(
                        2
                    )} €"
                    hideCloseButton
                />
            {:else if !forderungsstatus.sollstellungVorhanden && invalid}
                <InlineNotification
                    kind="error"
                    title="Kaltmiete zu hoch:"
                    subtitle="Maximal {kaltmieteMax?.toFixed(2)} € erlaubt"
                    hideCloseButton
                />
            {/if}
        </Column>
    </Row>
{/if}
