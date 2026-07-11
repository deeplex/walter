<!-- Copyright (C) 2023-2025  Kai Lawrence -->
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
        WalterBuchungssaetze,
        WalterHeaderDetail,
        WalterGrid,
        WalterLinks,
        WalterLinkTile,
        WalterTransaktionRaw
    } from '$walter/components';
    import type { PageData } from './$types';
    import { WalterFileWrapper } from '$walter/lib';
    import { fileURL } from '$walter/services/files';
    import { walter_delete, walter_post } from '$walter/services/requests';
    import { walter_goto } from '$walter/services/utils';
    import {
        Button,
        InlineNotification,
        Modal,
        TextArea
    } from 'carbon-components-svelte';
    import { TrashCan, Undo } from 'carbon-icons-svelte';

    export let data: PageData;

    let title = 'Transaktion';

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.fileURL);

    let stornoOpen = false;
    let stornoGrund = '';
    let loeschenOpen = false;
    let busy = false;
    let aktionsFehler: string | null = null;

    async function stornieren() {
        busy = true;
        aktionsFehler = null;
        try {
            const resp = await walter_post(
                `/api/transaktionen/${data.entry.id}/storno`,
                { grund: stornoGrund }
            );
            stornoOpen = false;
            if (resp.ok) {
                location.reload();
            } else {
                aktionsFehler = await resp.text();
            }
        } finally {
            busy = false;
        }
    }

    async function loeschen() {
        busy = true;
        aktionsFehler = null;
        try {
            const resp = await walter_delete(
                `/api/transaktionen/${data.entry.id}`
            );
            loeschenOpen = false;
            if (resp.ok) {
                await walter_goto('/transaktionen');
            } else {
                aktionsFehler = await resp.text();
            }
        } finally {
            busy = false;
        }
    }
</script>

<WalterHeaderDetail
    entry={data.entry}
    apiURL={data.apiURL}
    {title}
    bind:fileWrapper
/>

<WalterGrid>
    <WalterTransaktionRaw fetchImpl={data.fetchImpl} entry={data.entry} />

    {#if aktionsFehler}
        <InlineNotification
            kind="error"
            title="Korrektur nicht möglich:"
            subtitle={aktionsFehler}
            lowContrast
        />
    {/if}

    <div class="korrektur-aktionen">
        {#if data.entry.kannStornieren}
            <Button
                kind="danger-tertiary"
                icon={Undo}
                disabled={busy}
                on:click={() => (stornoOpen = true)}
            >
                Stornieren
            </Button>
        {/if}
        {#if data.entry.kannLoeschen}
            <Button
                kind="danger-tertiary"
                icon={TrashCan}
                disabled={busy}
                on:click={() => (loeschenOpen = true)}
            >
                Löschen
            </Button>
        {/if}
        {#if data.entry.sperrgrund}
            <InlineNotification
                kind="info"
                hideCloseButton
                title="Korrektur eingeschränkt:"
                subtitle={data.entry.sperrgrund}
                lowContrast
            />
        {/if}
    </div>

    <WalterLinks>
        <WalterBuchungssaetze
            fetchImpl={data.fetchImpl}
            title="Buchungssätze"
            rows={data.entry.buchungssaetze}
        />
    </WalterLinks>

    {#if data.entry.zahler}
        <WalterLinkTile
            bind:fileWrapper
            fileref={fileURL.kontakt(`${data.entry.zahler.id}`)}
            name={`Zahler: ${data.entry.zahler.text}`}
            href={`/kontakte/${data.entry.zahler.id}`}
        />
    {/if}

    {#if data.entry.zahlungsempfaenger}
        <WalterLinkTile
            bind:fileWrapper
            fileref={fileURL.kontakt(`${data.entry.zahlungsempfaenger.id}`)}
            name={`Zahlungsempfänger: ${data.entry.zahlungsempfaenger.text}`}
            href={`/kontakte/${data.entry.zahlungsempfaenger.id}`}
        />
    {/if}
</WalterGrid>

<Modal
    bind:open={stornoOpen}
    danger
    modalHeading="Transaktion stornieren"
    primaryButtonText="Stornieren"
    secondaryButtonText="Abbrechen"
    primaryButtonDisabled={busy || stornoGrund.trim().length === 0}
    on:click:button--secondary={() => (stornoOpen = false)}
    on:submit={stornieren}
>
    <p style="margin-bottom: 1rem;">
        Zu jedem Buchungssatz dieser Transaktion wird eine Gegenbuchung mit
        umgekehrten Soll/Haben-Seiten erstellt. Die Transaktion selbst bleibt
        als Beleg erhalten.
    </p>
    <TextArea
        labelText="Grund (Pflicht)"
        placeholder="Warum wird diese Transaktion storniert?"
        bind:value={stornoGrund}
    />
</Modal>

<Modal
    bind:open={loeschenOpen}
    danger
    modalHeading="Transaktion löschen"
    primaryButtonText="Endgültig löschen"
    secondaryButtonText="Abbrechen"
    primaryButtonDisabled={busy}
    on:click:button--secondary={() => (loeschenOpen = false)}
    on:submit={loeschen}
>
    <p>
        Die Transaktion und alle zugehörigen Buchungssätze werden endgültig
        entfernt. Das ist nur für frische Fehleingaben gedacht — im Zweifel
        lieber stornieren, damit die Korrektur nachvollziehbar bleibt.
    </p>
</Modal>

<style>
    .korrektur-aktionen {
        align-items: center;
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
        padding: 0 1rem 1rem;
    }
</style>
