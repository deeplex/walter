<script lang="ts">
    import { isWalterSideNavOpen } from '$walter/store';

    import {
        Loading,
        SideNav,
        SideNavDivider,
        SideNavItems,
        SideNavLink,
        SideNavMenu
    } from 'carbon-components-svelte';
    import {
        Building,
        ChartRelationship,
        Document,
        Home,
        IbmDataReplication,
        ListBoxes,
        Location,
        Logout,
        Meter,
        Money,
        NetworkAdminControl,
        Tools,
        TrashCan,
        User,
        UserMultiple
    } from 'carbon-icons-svelte';
    import { checkStackTodo, logout } from './WalterSideNav';
    import WalterSideNavLink from './WalterSideNavLink.svelte';
    import { UserRole, authState } from '$walter/services/auth';
    import { get } from 'svelte/store';

    export let fetchImpl: typeof fetch;

    let winWidth = 0;
    let isOpen: boolean;
    isWalterSideNavOpen.subscribe((value) => (isOpen = value));

    function closeSideNav() {
        isOpen = false;
    }

    let username: string | undefined;
    $: {
        username = (authState && get(authState)?.name) || username;
    }
    authState?.subscribe((e) => (username = e?.name));
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
        <SideNavDivider />
        <div>
            <SideNavMenu icon={ListBoxes} text="Erweitert">
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
                <!-- </div> -->
            </SideNavMenu>
        </div>

        <li style="flex: 1" />

        <SideNavDivider />
        <div>
            <SideNavMenu id="usermenu" icon={User} text={username}>
                {#await checkStackTodo(fetchImpl)}
                    <WalterSideNavLink
                        icon={Loading}
                        text="Ablagestapel"
                        href="/stack"
                    />
                {:then x}
                    <WalterSideNavLink
                        icon={x}
                        text="Ablagestapel"
                        href="/stack"
                    />
                {/await}

                <WalterSideNavLink
                    text="Nutzereinstellungen"
                    icon={User}
                    href="/user"
                />
                {#if authState && get(authState)?.role === UserRole.Admin}
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
            </SideNavMenu>
        </div>
    </SideNavItems>
    <!-- To get the sidenav scrollable when window height is very small -->
    <div style="height: 4em" />
</SideNav>
