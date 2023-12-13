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
