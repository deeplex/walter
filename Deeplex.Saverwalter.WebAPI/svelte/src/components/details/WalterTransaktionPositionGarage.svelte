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
        InlineLoading,
        Tag
    } from 'carbon-components-svelte';
    import {
        WalterComboBox,
        WalterMonthPicker,
        WalterNumberInput
    } from '$walter/components';
    import { walter_selection, walter_get } from '$walter/services/requests';
    import type { WalterSelectionEntry } from '$walter/lib';
    import type { StandaloneGaragenmietInput } from '$walter/lib';

    export let fetchImpl: typeof fetch;
    export let garage: StandaloneGaragenmietInput;
    export let invalid = false;
    export let statusText = '';

    interface GarageForderungsstatus {
        garageVertragId: number;
        garageKennung: string;
        garagenMiete: number;
        schonGezahlt: number;
        verbleibendeForderung: number;
        sollstellungVorhanden: boolean;
    }

    let isLoading = false;
    let garageVertrag: WalterSelectionEntry | undefined = undefined;
    let forderungsstatus: GarageForderungsstatus | undefined = undefined;

    const garageVertraege = walter_selection.garageVertraege(fetchImpl);

    const now = new Date();
    let betreffenderMonat: string =
        garage.betreffenderMonat ||
        `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-01`;
    garage.betreffenderMonat = betreffenderMonat;

    $: invalid = !garage.garageVertragId;

    $: statusText = forderungsstatus?.sollstellungVorhanden
        ? `Bereits gezahlt: ${forderungsstatus.schonGezahlt.toFixed(2)} € — Verbleibend: ${forderungsstatus.verbleibendeForderung.toFixed(2)} €`
        : '';

    $: maxBetrag = forderungsstatus
        ? forderungsstatus.sollstellungVorhanden
            ? forderungsstatus.verbleibendeForderung
            : forderungsstatus.garagenMiete
        : undefined;

    async function ladeForderungsstatus() {
        if (!garage.garageVertragId || !betreffenderMonat) return;
        isLoading = true;
        try {
            const resp = await walter_get(
                `/api/garage-vertraege/${garage.garageVertragId}/forderung/${betreffenderMonat}`,
                fetchImpl
            );
            if (resp && typeof resp === 'object' && 'garagenMiete' in resp) {
                forderungsstatus = resp as GarageForderungsstatus;
                if (forderungsstatus.sollstellungVorhanden) {
                    garage.betrag = forderungsstatus.verbleibendeForderung;
                } else {
                    garage.betrag = forderungsstatus.garagenMiete;
                }
            }
        } catch {
            /* ignore */
        } finally {
            isLoading = false;
        }
    }

    function onGarageVertragSelect(e: CustomEvent) {
        garageVertrag = e.detail?.selectedItem;
        garage.garageVertragId = (garageVertrag?.id as number) ?? 0;
        garage.garageKennung = garageVertrag?.text ?? '';
        forderungsstatus = undefined;
        ladeForderungsstatus();
    }

    $: if (betreffenderMonat !== undefined) {
        garage.betreffenderMonat = betreffenderMonat;
        ladeForderungsstatus();
    }
</script>

<Row>
    <Column>
        <WalterComboBox
            required
            titleText="Garagenvertrag"
            entries={garageVertraege}
            bind:value={garageVertrag}
            initialId={garage.garageVertragId || undefined}
            on:select={onGarageVertragSelect}
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
        {#if isLoading}
            <InlineLoading description="Forderungsstatus wird geladen…" />
        {:else}
            <WalterNumberInput
                required
                label={forderungsstatus?.garagenMiete
                    ? forderungsstatus.sollstellungVorhanden
                        ? `Betrag (€) — Grundmiete: ${forderungsstatus.garagenMiete.toFixed(2)} € — Verbleibend: ${forderungsstatus.verbleibendeForderung.toFixed(2)} €`
                        : `Betrag (€) — Grundmiete: ${forderungsstatus.garagenMiete.toFixed(2)} €`
                    : 'Betrag (€)'}
                bind:value={garage.betrag}
            />
        {/if}
    </Column>
</Row>

{#if forderungsstatus && maxBetrag !== undefined && garage.betrag > maxBetrag + 0.005}
    <Row>
        <Column>
            <InlineNotification
                kind="error"
                title="Betrag zu hoch:"
                subtitle="Maximal {maxBetrag.toFixed(2)} € erlaubt"
                hideCloseButton
            />
        </Column>
    </Row>
{/if}
