<script lang="ts">
    import { goto } from '$app/navigation';
    import { page } from '$app/stores';
    import { isWalterSideNavOpen } from '$WalterStore';

    import {
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
        Location,
        Logout,
        Meter,
        Money,
        Settings,
        Tools,
        User,
        UserMultiple
    } from 'carbon-icons-svelte';
    import { WalterToastContent } from '$WalterLib';
    import { walter_sign_out } from '$WalterServices/auth';

    let winWidth = 0;
    let isOpen: boolean;
    isWalterSideNavOpen.subscribe((value) => (isOpen = value));

    function closeSideNavIfWinWidthSmall() {
        // https://github.com/carbon-design-system/carbon-components-svelte/blob/master/src/UIShell/Header.svelte#L44
        if (winWidth < 1056) {
            closeSideNav();
        }
    }

    function closeSideNav() {
        isWalterSideNavOpen.update((e: any) => false);
    }

    function logout() {
        const LogoutToast = new WalterToastContent('Abmeldung erfolgreich');
        walter_sign_out(LogoutToast);
        goto('/login');
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
        <SideNavLink
            on:click={closeSideNavIfWinWidthSmall}
            isSelected={$page.route.id?.includes('/betriebskostenrechnungen')}
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

        <SideNavDivider />

        <SideNavMenu text="Einstellungen" icon={Settings}>
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
</SideNav>
