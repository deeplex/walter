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
        WalterTextArea,
        WalterComboBoxKontakt,
        WalterComboBoxWohnung,
        WalterMultiSelectKontakt
    } from '$walter/components';
    import type { WalterVertragEntry } from '$walter/lib';
    import { Row, TextInput, InlineNotification } from 'carbon-components-svelte';
    import { convertDateGerman } from '$walter/services/utils';
    import { walter_fetch } from '$walter/services/requests';

    export let entry: Partial<WalterVertragEntry> = {};
    export let fetchImpl: typeof fetch;
    export let readonly = false;
    export let beginn: string | undefined = undefined;
    $: {
        readonly = entry?.permissions?.update === false;
    }

    type OverlapConflict = { id: number; beginn: string; ende: string | null; mieter: string };

    async function fetchOverlap(wohnungId: number | undefined, beginn: string | undefined, ende: string | undefined, excludeId: number | undefined): Promise<OverlapConflict | null> {
        if (!wohnungId || !beginn) return null;
        const params = new URLSearchParams({ wohnungId: String(wohnungId), beginn });
        if (ende) params.set('ende', ende);
        if (excludeId) params.set('excludeId', String(excludeId));
        try {
            const response = await walter_fetch(fetchImpl, `/api/vertraege/check-overlap?${params}`);
            return await response.json();
        } catch {
            return null;
        }
    }

    $: overlapPromise = fetchOverlap(
        entry.wohnung?.id,
        beginn ?? entry.beginn,
        entry.ende,
        entry.id
    );
</script>

{#await overlapPromise then overlap}
    {#if overlap}
        <InlineNotification kind="error" title="Konflikt:" hideCloseButton>
            <svelte:fragment slot="subtitle">
                Konflikt mit bestehendem Vertrag
                <a href="/vertraege/{overlap.id}">{overlap.mieter || 'Unbekannt'}</a>
                vom {new Date(overlap.beginn).toLocaleDateString('de-DE')}
                bis {overlap.ende ? new Date(overlap.ende).toLocaleDateString('de-DE') : 'offen'}.
            </svelte:fragment>
        </InlineNotification>
    {/if}
{/await}
<Row>
    <TextInput
        placeholder="Wird aus Nachtrag genommen"
        required
        readonly
        value={entry.beginn && convertDateGerman(new Date(entry.beginn))}
        labelText="Beginn (aus Nachtrag)"
    />
    <WalterDatePicker
        disabled={readonly}
        bind:value={entry.ende}
        labelText="Ende"
        placeholder="Offen"
    />
</Row>
<Row>
    <WalterComboBoxWohnung
        {fetchImpl}
        required
        {readonly}
        bind:value={entry.wohnung}
    />
    <TextInput labelText="Vermieter" readonly value={entry.wohnung?.filter} />
    <WalterComboBoxKontakt
        {fetchImpl}
        {readonly}
        bind:value={entry.ansprechpartner}
        title="Ansprechpartner"
    />
</Row>
<Row>
    <WalterMultiSelectKontakt
        {fetchImpl}
        {readonly}
        bind:value={entry.selectedMieter}
        titleText="Mieter"
    />
</Row>
<Row>
    <WalterTextArea {readonly} labelText="Notiz" bind:value={entry.notiz} />
</Row>
