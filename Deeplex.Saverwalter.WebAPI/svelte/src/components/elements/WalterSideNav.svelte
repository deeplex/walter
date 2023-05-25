<script lang="ts">
    import { goto } from '$app/navigation';
    import { page } from '$app/stores';

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

    function logout() {
        const LogoutToast = new WalterToastContent('Abmeldung erfolgreich');
        walter_sign_out(LogoutToast);
        goto('/login');
    }
</script>

<SideNav rail>
    <SideNavItems>
        <SideNavLink
            isSelected={$page.route.id?.includes('/kontakte')}
            icon={UserMultiple}
            text="Kontakte"
            href="/kontakte"
        />

        <SideNavLink
            isSelected={$page.route.id?.includes('/wohnungen')}
            icon={Building}
            text="Wohnungen"
            href="/wohnungen"
        />
        <SideNavLink
            isSelected={$page.route.id?.includes('/vertraege')}
            icon={Document}
            text="Verträge"
            href="/vertraege"
        />
        <SideNavLink
            isSelected={$page.route.id?.includes('/betriebskostenrechnungen')}
            icon={Money}
            text="Betriebskostenrechnungen"
            href="/betriebskostenrechnungen"
        />
        <SideNavLink
            isSelected={$page.route.id?.includes('/erhaltungsaufwendungen')}
            icon={Tools}
            text="Erhaltungsaufwendungen"
            href="/erhaltungsaufwendungen"
        />
        <SideNavLink
            isSelected={$page.route.id?.includes('/umlagen')}
            icon={ChartRelationship}
            text="Umlagen"
            href="/umlagen"
        />
        <SideNavLink
            isSelected={$page.route.id?.includes('/zaehler')}
            icon={Meter}
            text="Zähler"
            href="/zaehler"
        />

        <SideNavLink
            isSelected={$page.route.id?.includes('/adressen')}
            icon={Location}
            text="Adressen"
            href="/adressen"
        />

        <SideNavDivider />

        <SideNavMenu text="Einstellungen" icon={Settings}>
            <SideNavLink
                isSelected={$page.route.id?.includes('/account')}
                text="Nutzereinstellungen"
                icon={User}
                href="/account"
                style="padding-left: 1em"
            />
            <SideNavLink
                icon={Logout}
                on:click={logout}
                text="Abmelden"
                style="padding-left: 1em"
            />
        </SideNavMenu>
    </SideNavItems>
</SideNav>
