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
        WalterBuchhaltungAbschnitt,
        WalterBuchungssaetze,
        WalterHeaderDetail,
        WalterGrid,
        WalterLinkTile,
        WalterTransaktionRaw
    } from '$walter/components';
    import WalterRueckgaengig from '$walter/components/details/WalterRueckgaengig.svelte';
    import type { PageData } from './$types';
    import { WalterFileWrapper, type RueckgaengigPlan } from '$walter/lib';
    import { fileURL } from '$walter/services/files';
    import { walter_goto } from '$walter/services/utils';

    export let data: PageData;

    let title = 'Transaktion';

    let fileWrapper = new WalterFileWrapper(data.fetchImpl);
    fileWrapper.registerStack();
    fileWrapper.register(title, data.fileURL);

    async function erledigt(e: CustomEvent<RueckgaengigPlan>) {
        if (e.detail.art === 'Loeschen') await walter_goto('/transaktionen');
        else location.reload();
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

    {#if data.entry.permissions?.remove !== false}
        <div class="korrektur-aktionen">
            <WalterRueckgaengig
                fetchImpl={data.fetchImpl}
                transaktionId={data.entry.id}
                kind="danger-tertiary"
                size="default"
                on:erledigt={erledigt}
            />
        </div>
    {/if}

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

    <WalterBuchhaltungAbschnitt>
        <WalterBuchungssaetze
            fetchImpl={data.fetchImpl}
            rows={data.entry.buchungssaetze}
        />
    </WalterBuchhaltungAbschnitt>
</WalterGrid>

<style>
    .korrektur-aktionen {
        align-items: center;
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
        padding: 0 1rem 1rem;
    }
</style>
