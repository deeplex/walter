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
    import { isWalterSideNavOpen } from '$walter/store';
    import { Button, Header } from 'carbon-components-svelte';
    import { Close, Menu } from 'carbon-icons-svelte';

    export let title: string;

    let isSideNavOpen = true;

    isWalterSideNavOpen.subscribe((value) => {
        isSideNavOpen = value;
    });

    function toggleWalterSideNav() {
        isWalterSideNavOpen.update((value) => !value);
    }
</script>

<Header platformName={title}>
    <svelte:fragment slot="skip-to-content">
        <!--
            padding-bottom because the button has 11 per default -1 for countering the margin. The margin is so the top border is visible.
        -->
        <Button
            style="background-color: #161616; margin-bottom: 1px; padding-bottom: 10px"
            iconDescription="Navigation"
            tooltipPosition="top"
            kind="secondary"
            icon={isSideNavOpen ? Close : Menu}
            on:click={toggleWalterSideNav}
        />
    </svelte:fragment>
    <slot />
</Header>
