<script lang="ts">
    // import 'carbon-components-svelte/css/white.css';
    import 'carbon-components-svelte/css/all.css';

    import { WalterSideNav } from '$walter/components';
    import { Modal } from 'carbon-components-svelte';
    import type { WalterModalControl } from '$walter/types';
    import { walterModalControl } from '$walter/store';
    import { getAccessToken } from '$walter/services/auth';
    import { goto } from '$app/navigation';
    import type { PageData } from './$types';

    export let data: PageData;

    if (getAccessToken() == null) {
        goto('/login');
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
