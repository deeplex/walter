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
    import { page } from '$app/stores';
    import { walter_goto } from '$walter/services/utils';
    import { isWalterSideNavOpen } from '$walter/store';
    import { Loading, SideNavLink } from 'carbon-components-svelte';
    import type { CarbonIcon } from 'carbon-icons-svelte';

    let winWidth = 0;

    export let icon: typeof CarbonIcon | typeof Loading;
    export let text: string;
    export let href: string;

    function closeSideNavIfWinWidthSmall(e: Event) {
        e.preventDefault();
        walter_goto(href);
        // https://github.com/carbon-design-system/carbon-components-svelte/blob/master/src/UIShell/Header.svelte#L44
        if (winWidth < 1056) {
            isWalterSideNavOpen.update(() => false);
        }
    }
</script>

<svelte:window bind:innerWidth={winWidth} />

<SideNavLink
    on:click={closeSideNavIfWinWidthSmall}
    isSelected={href === `/${$page.url.pathname.split('/')[1]}`}
    {icon}
    {text}
    {href}
/>
