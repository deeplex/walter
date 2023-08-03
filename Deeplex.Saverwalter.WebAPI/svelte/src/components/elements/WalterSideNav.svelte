<script lang="ts">
    import { goto } from '$app/navigation';
    import { page } from '$app/stores';
    import { isWalterSideNavOpen } from '$walter/store';

    import {
        Checkbox,
        Loading,
        SideNav,
        SideNavDivider,
        SideNavItems,
        SideNavLink,
        SideNavMenu,
        SideNavMenuItem
    } from 'carbon-components-svelte';
    import {
        Building,
        ChartRelationship,
        Document,
        Location,
        Logout,
        Meter,
        Money,
        Settings,
        Tools,
        TrashCan,
        User,
        UserMultiple
    } from 'carbon-icons-svelte';
    import { checkStackTodo, logout } from './WalterSideNav';

    export let fetchImpl: typeof fetch;

    let winWidth = 0;
    let isOpen: boolean;
    isWalterSideNavOpen.subscribe((value) => (isOpen = value));

    let extendedNavigation = true;

    function closeSideNavIfWinWidthSmall() {
        // https://github.com/carbon-design-system/carbon-components-svelte/blob/master/src/UIShell/Header.svelte#L44
        if (winWidth < 1056) {
            closeSideNav();
        }
    }

    export function closeSideNav() {
        isWalterSideNavOpen.update((_e: unknown) => false);
    }
</script>

<svelte:window bind:innerWidth={winWidth} />

<SideNav bind:isOpen on:close={closeSideNav}>
    <SideNavItems>
        <SideNavLink
            on:click={closeSideNavIfWinWidthSmall}
            isSelected={$page.route.id?.includes('/kontakte')}
            icon={UserMultiple}
            text="Kontakte"
            href="/kontakte"
        />

        <SideNavLink
            on:click={closeSideNavIfWinWidthSmall}
            isSelected={$page.route.id?.includes('/wohnungen')}
            icon={Building}
            text="Wohnungen"
            href="/wohnungen"
        />

        <SideNavLink
            on:click={closeSideNavIfWinWidthSmall}
            isSelected={$page.route.id?.includes('/vertraege')}
            icon={Document}
            text="Verträge"
            href="/vertraege"
        />

        {#if extendedNavigation}
            <SideNavDivider />

            <SideNavLink
                on:click={closeSideNavIfWinWidthSmall}
                isSelected={$page.route.id?.includes(
                    '/betriebskostenrechnungen'
                )}
                icon={Money}
                text="Betriebskostenrechnungen"
                href="/betriebskostenrechnungen"
            />

            <SideNavLink
                on:click={closeSideNavIfWinWidthSmall}
                isSelected={$page.route.id?.includes('/erhaltungsaufwendungen')}
                icon={Tools}
                text="Erhaltungsaufwendungen"
                href="/erhaltungsaufwendungen"
            />

            <SideNavLink
                on:click={closeSideNavIfWinWidthSmall}
                isSelected={$page.route.id?.includes('/umlagen')}
                icon={ChartRelationship}
                text="Umlagen"
                href="/umlagen"
            />

            <SideNavLink
                on:click={closeSideNavIfWinWidthSmall}
                isSelected={$page.route.id?.includes('/zaehler')}
                icon={Meter}
                text="Zähler"
                href="/zaehler"
            />

            <SideNavLink
                on:click={closeSideNavIfWinWidthSmall}
                isSelected={$page.route.id?.includes('/adressen')}
                icon={Location}
                text="Adressen"
                href="/adressen"
            />
        {/if}

        <SideNavDivider />

        {#await checkStackTodo(fetchImpl)}
            <SideNavLink
                on:click={closeSideNavIfWinWidthSmall}
                isSelected={$page.route.id?.includes('/stack')}
                icon={Loading}
                text="Ablagestapel"
                href="/stack"
            />
        {:then x}
            <SideNavLink
                on:click={closeSideNavIfWinWidthSmall}
                isSelected={$page.route.id?.includes('/stack')}
                icon={x}
                text="Ablagestapel"
                href="/stack"
            />
        {/await}

        {#if extendedNavigation}
        <SideNavLink
            on:click={closeSideNavIfWinWidthSmall}
            isSelected={$page.route.id?.includes('/trash')}
            icon={TrashCan}
            text="Papierkorb"
            href="/trash" />
        {/if}
  
        <SideNavDivider />

        <SideNavMenu text="Einstellungen" icon={Settings}>
            <SideNavMenuItem style="padding-left: 1.2em">
                <Checkbox
                    bind:checked={extendedNavigation}
                    labelText="Erweiterte Navigation"
                />
            </SideNavMenuItem>

            <SideNavLink
                on:click={closeSideNavIfWinWidthSmall}
                isSelected={$page.route.id?.includes('/account')}
                text="Nutzereinstellungen"
                icon={User}
                href="/account"
                style="padding-left: 1em"
            />
            <SideNavLink
                on:click={closeSideNavIfWinWidthSmall}
                icon={Logout}
                on:click={logout}
                text="Abmelden"
                style="padding-left: 1em"
            />
        </SideNavMenu>
    </SideNavItems>
    <!-- To get the sidenav scrollable when window height is very small -->
    <div style="height: 4em" />
</SideNav>
