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
    import { Button, NumberInput, Tile } from 'carbon-components-svelte';
    import Download from 'carbon-icons-svelte/lib/Download.svelte';
    import { walter_post } from '$walter/services/requests';
    import { download_file_blob } from '$walter/services/files';

    export let wohnungId: number;

    let jahr = new Date().getFullYear() - 1;
    let loading = false;
    let errorMsg = '';

    async function drucken() {
        loading = true;
        errorMsg = '';
        try {
            const response = await walter_post(
                '/api/abrechnungslauf/print/pdf',
                {
                    wohnungIds: [wohnungId],
                    jahr
                }
            );
            if (!response.ok) {
                const text = await response.text();
                errorMsg = text || `Fehler ${response.status}`;
                return;
            }
            const disposition =
                response.headers.get('content-disposition') ?? '';
            const match = disposition.match(
                /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/
            );
            const fileName = match
                ? match[1].replace(/['"]/g, '')
                : `NK_${jahr}_Abrechnung.pdf`;
            const blob = await response.blob();
            download_file_blob(blob, fileName);
        } finally {
            loading = false;
        }
    }
</script>

<Tile
    light
    style="padding: 1rem; display: flex; flex-direction: column; gap: 0.75rem;"
>
    <strong>Betriebskostenabrechnung drucken</strong>
    <NumberInput
        label="Abrechnungsjahr"
        bind:value={jahr}
        min={2000}
        max={new Date().getFullYear()}
        hideSteppers
    />
    {#if errorMsg}
        <p style="color: var(--cds-support-error);">{errorMsg}</p>
    {/if}
    <Button icon={Download} disabled={loading} on:click={drucken} size="small">
        {loading ? 'Wird erstellt…' : 'PDF herunterladen'}
    </Button>
</Tile>
