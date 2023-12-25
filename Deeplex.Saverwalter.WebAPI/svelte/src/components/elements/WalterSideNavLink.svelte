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
