<script lang="ts">
    import { page } from "$app/stores";
    import { isWalterSideNavOpen } from "$walter/store";
    import { SideNavLink } from "carbon-components-svelte";
    import type { SvelteComponent } from "svelte";

    let winWidth = 0;

    export let icon: typeof SvelteComponent;
    export let text: string;
    export let href: string;

    function closeSideNavIfWinWidthSmall() {
        // https://github.com/carbon-design-system/carbon-components-svelte/blob/master/src/UIShell/Header.svelte#L44
        if (winWidth < 1056) {
            isWalterSideNavOpen.update((_e: unknown) => false);
        }
    }
</script>

<svelte:window bind:innerWidth={winWidth} />

<SideNavLink
    on:click={closeSideNavIfWinWidthSmall}
    isSelected={$page.route.id?.includes(href)}
    {icon}
    {text}
    {href}
/>
