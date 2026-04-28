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
    import { browser } from '$app/environment';
    import {
        WalterError,
        WalterGrid,
        WalterHeader
    } from '$walter/components';
    import { ClickableTile } from 'carbon-components-svelte';
    import { UserRole, getAuthState } from '$walter/services/auth';

    const currentAuthState = browser ? getAuthState() : undefined;
</script>

{#if currentAuthState && $currentAuthState?.role === UserRole.Admin}
    <WalterHeader title="Adminbereich" />
    <WalterGrid>
        <ClickableTile href="/accounts">Nutzeraccounts</ClickableTile>
    </WalterGrid>
{:else}
    <WalterHeader title="Fehler" />
    <WalterError />
{/if}
