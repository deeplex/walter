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
        WalterDatePicker,
        WalterNumberInput
    } from '$walter/components';
    import type {
        WalterGarageVertragEntry,
        WalterGarageVertragVersionEntry
    } from '$walter/lib';
    import { walter_fetch } from '$walter/services/requests';
    import { InlineNotification, Row } from 'carbon-components-svelte';

    export let entry: Partial<WalterGarageVertragVersionEntry> = {};
    export let garageVertrag: Partial<WalterGarageVertragEntry> | undefined =
        undefined;
    export let beginn: string | undefined = undefined;
    export let hasOverlap = false;
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    $: {
        readonly = garageVertrag?.permissions?.update === false;
    }

    $: if (entry && beginn !== undefined) entry.beginn = beginn;

    $: if (garageVertrag?.garage?.id && entry.beginn) {
        const garageId = garageVertrag.garage.id;
        const b = entry.beginn;
        const e = garageVertrag.ende;
        const excludeId = garageVertrag.id ?? 0;

        const params = new URLSearchParams({
            garageId: String(garageId),
            beginn: b,
            excludeId: String(excludeId)
        });
        if (e) params.set('ende', e);

        walter_fetch(
            fetchImpl,
            `/api/garage-vertraege/check-overlap?${params}`,
            { method: 'GET' }
        )
            .then((r) => r.json())
            .then((conflict) => {
                hasOverlap = conflict !== null;
                overlapInfo = conflict;
            })
            .catch(() => {
                hasOverlap = false;
                overlapInfo = null;
            });
    }

    let overlapInfo: { beginn: string; ende?: string; mieter: string } | null =
        null;
</script>

<Row>
    <WalterDatePicker
        disabled={readonly}
        bind:value={entry.beginn}
        bind:valueRaw={beginn}
        labelText="Beginn"
        required
    />
    <WalterNumberInput
        disabled={readonly}
        bind:value={entry.garagenMiete}
        label="Garagenmiete (€)"
        required
    />
</Row>
{#if hasOverlap && overlapInfo}
    <Row>
        <InlineNotification
            kind="error"
            title="Überschneidung:"
            subtitle={`Garage bereits vermietet von ${overlapInfo.mieter} ab ${overlapInfo.beginn}${overlapInfo.ende ? ' bis ' + overlapInfo.ende : ''}`}
        />
    </Row>
{/if}
