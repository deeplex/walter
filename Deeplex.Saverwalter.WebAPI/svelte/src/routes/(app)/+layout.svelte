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
    // import 'carbon-components-svelte/css/white.css';
    import 'carbon-components-svelte/css/all.css';

    import { WalterSideNav } from '$walter/components';
    import { Modal } from 'carbon-components-svelte';
    import type { WalterModalControl } from '$walter/types';
    import { walterModalControl } from '$walter/store';
    import { getAccessToken } from '$walter/services/auth';
    import type { PageData } from './$types';
    import { walter_goto } from '$walter/services/utils';

    export let data: PageData;

    if (getAccessToken() == null) {
        walter_goto('/login');
    }

    let modalControl: WalterModalControl;
    walterModalControl.subscribe((value) => {
        modalControl = value;
    });
</script>

<WalterSideNav fetchImpl={data.fetch} />
<Modal
    {...modalControl}
    bind:open={modalControl.open}
    secondaryButtonText="Abbrechen"
    on:click:button--secondary={() => (modalControl.open = false)}
    on:click:button--primary={() => (modalControl.open = false)}
    on:open
    on:close
    on:submit={modalControl.submit}
>
    <p>{modalControl.content}</p>
</Modal>
<slot />
