<script lang="ts">
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
        Home,
        IbmDataReplication,
        Location,
        Logout,
        Meter,
        Money,
        NetworkAdminControl,
        Settings,
        Tools,
        TrashCan,
        User,
        UserMultiple
    } from 'carbon-icons-svelte';
    import { checkStackTodo, logout } from './WalterSideNav';
    import WalterSideNavLink from './WalterSideNavLink.svelte';

    export let fetchImpl: typeof fetch;

    let winWidth = 0;
    let isOpen: boolean;
    isWalterSideNavOpen.subscribe((value) => (isOpen = value));

    let extendedNavigation = true;

    function closeSideNav() {
        isOpen = false;
    }
</script>

<svelte:window bind:innerWidth={winWidth} />

<SideNav bind:isOpen on:close={closeSideNav}>
    <SideNavItems>
        <WalterSideNavLink icon={Home} text="Startseite" href="/" />
        <WalterSideNavLink
            icon={Document}
            text="Abrechnung"
            href="/abrechnung"
        />
        <SideNavDivider />
        <WalterSideNavLink
            icon={UserMultiple}
            text="Kontakte"
            href="/kontakte"
        />
        <WalterSideNavLink icon={Building} text="Wohnungen" href="/wohnungen" />
        <WalterSideNavLink icon={Document} text="Verträge" href="/vertraege" />

        {#if extendedNavigation}
            <SideNavDivider />
            <WalterSideNavLink
                icon={Money}
                text="Betriebskostenrechnungen"
                href="/betriebskostenrechnungen"
            />
            <WalterSideNavLink
                icon={ChartRelationship}
                text="Umlagen"
                href="/umlagen"
            />
            <WalterSideNavLink
                icon={IbmDataReplication}
                text="Umlagetypen"
                href="/umlagetypen"
            />
            <WalterSideNavLink
                icon={Tools}
                text="Erhaltungsaufwendungen"
                href="/erhaltungsaufwendungen"
            />
            <WalterSideNavLink icon={Meter} text="Zähler" href="/zaehler" />
            <WalterSideNavLink
                icon={Location}
                text="Adressen"
                href="/adressen"
            />
        {/if}

        <SideNavDivider />

        {#await checkStackTodo(fetchImpl)}
            <WalterSideNavLink
                icon={Loading}
                text="Ablagestapel"
                href="/stack"
            />
        {:then x}
            <WalterSideNavLink icon={x} text="Ablagestapel" href="/stack" />
        {/await}

        {#if extendedNavigation}
            <WalterSideNavLink
                icon={TrashCan}
                text="Papierkorb"
                href="/trash"
            />
        {/if}

        <li style="flex: 1" />

        <SideNavDivider />
        <li>
            <Checkbox
                labelText="Erweiterte Navigation"
                bind:checked={extendedNavigation}
            />
        </li>
        <WalterSideNavLink
            text="Nutzereinstellungen"
            icon={User}
            href="/user"
        />
        {#if true}
            <WalterSideNavLink
                text="Adminbereich"
                icon={NetworkAdminControl}
                href="/admin"
            />
        {/if}
        <SideNavLink
            icon={Logout}
            on:click={logout}
            text="Abmelden"
            style="padding-left: 1em"
        />
    </SideNavItems>
    <!-- To get the sidenav scrollable when window height is very small -->
    <div style="height: 4em" />
</SideNav>
