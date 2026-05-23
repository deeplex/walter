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
    import {
        WalterDatePicker,
        WalterNumberInput,
        WalterTextArea
    } from '$walter/components';
    import { InlineNotification, Row } from 'carbon-components-svelte';
    import type {
        WalterVertragEntry,
        WalterVertragVersionEntry
    } from '$walter/lib';
    import { walter_fetch } from '$walter/services/requests';

    export let entry: Partial<WalterVertragVersionEntry> = {};
    export let vertrag: Partial<WalterVertragEntry>;
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    export let beginn: string | undefined = entry.beginn;
    export let hasOverlap = false;

    async function checkOverlap(
        wohnungId: string | number | undefined,
        b: string | undefined,
        ende: string | undefined,
        excludeId: string | number | undefined
    ) {
        if (!wohnungId || !b) {
            overlapConflict = null;
            hasOverlap = false;
            return;
        }
        const params = new URLSearchParams({
            wohnungId: String(wohnungId),
            beginn: b
        });
        if (ende) params.set('ende', ende);
        if (excludeId) params.set('excludeId', String(excludeId));
        try {
            const response = await walter_fetch(
                fetchImpl,
                `/api/vertraege/check-overlap?${params}`
            );
            overlapConflict = await response.json();
        } catch {
            overlapConflict = null;
        }
        hasOverlap = !!overlapConflict;
    }

    $: checkOverlap(vertrag?.wohnung?.id, beginn, vertrag?.ende, vertrag?.id);

    $: {
        readonly = entry?.permissions?.update === false;
    }
    $: entry.beginn = beginn;

    type OverlapConflict = {
        id: number;
        beginn: string;
        ende: string | null;
        mieter: string;
    };
    let overlapConflict: OverlapConflict | null = null;
</script>

<Row>
    <WalterDatePicker
        required
        disabled={readonly}
        bind:value={beginn}
        labelText="Beginn"
    />
    <WalterNumberInput
        required
        {readonly}
        hideSteppers
        bind:value={entry.grundmiete}
        label="Grundmiete"
    />
    <WalterNumberInput
        {readonly}
        hideSteppers
        bind:value={entry.nebenkostenvorauszahlung}
        label="Nebenkostenvorauszahlung"
    />
    <WalterNumberInput
        required
        {readonly}
        bind:value={entry.personenzahl}
        label="Personenzahl"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>

{#if overlapConflict}
    <InlineNotification kind="error" title="Konflikt:" hideCloseButton>
        <svelte:fragment slot="subtitle">
            Konflikt mit bestehendem Vertrag
            <a href="/vertraege/{overlapConflict.id}"
                >{overlapConflict.mieter || 'Unbekannt'}</a
            >
            vom {new Date(overlapConflict.beginn).toLocaleDateString('de-DE')}
            bis {overlapConflict.ende
                ? new Date(overlapConflict.ende).toLocaleDateString('de-DE')
                : 'offen'}.
        </svelte:fragment>
    </InlineNotification>
{/if}
